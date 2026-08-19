using Application.Storage;
using Application.Translations;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Blogs;

/// <summary>
/// Madde 21/21.1 — Blog Modülü. Kapak görseli (FeaturedImage) ProductImage/ReferenceProjectImage gibi
/// ayrı bir tabloya değil, doğrudan Blog.FeaturedImagePath'e yazılır (tek görsel, galeri değil) —
/// ama doğrulama/güvenlik ProductImageService ile birebir aynıdır (uzantı whitelist, MIME çapraz
/// kontrol, magic-byte, GUID dosya adı, IFileStorageService, telafi mantığı).
/// </summary>
public class BlogService
{
    private const string TrLanguageCode = "TR";
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string> AllowedExtensionContentTypes =
        new Dictionary<string, string>
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

    private readonly IBlogRepository _blogRepository;
    private readonly IBlogCategoryRepository _blogCategoryRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ITranslationService _translationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BlogService> _logger;

    public BlogService(
        IBlogRepository blogRepository,
        IBlogCategoryRepository blogCategoryRepository,
        ITagRepository tagRepository,
        ITranslationService translationService,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<BlogService> logger)
    {
        _blogRepository = blogRepository;
        _blogCategoryRepository = blogCategoryRepository;
        _tagRepository = tagRepository;
        _translationService = translationService;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync() =>
        _translationService.GetActiveLanguagesAsync();

    public async Task<IReadOnlyList<(int Id, string Label)>> GetBlogCategoryOptionsAsync()
    {
        var categories = await _blogCategoryRepository.GetAllAsync();
        var result = new List<(int Id, string Label)>();

        foreach (var category in categories.OrderBy(c => c.DisplayOrder))
        {
            var translations = await _translationService.GetTranslationsAsync(EntityType.BlogCategory, category.Id);
            var name = translations
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == BlogCategoryFields.Name)?.Value;

            result.Add((category.Id, name ?? $"#{category.Id}"));
        }

        return result;
    }

    public async Task<IReadOnlyList<BlogDto>> GetAllAsync()
    {
        var blogs = await _blogRepository.GetAllAsync();

        var result = new List<BlogDto>();
        foreach (var blog in blogs)
        {
            result.Add(await MapToDtoAsync(blog));
        }

        return result;
    }

    public async Task<BlogDto?> GetByIdAsync(int id)
    {
        var blog = await _blogRepository.GetByIdAsync(id);
        return blog is null ? null : await MapToDtoAsync(blog);
    }

    public async Task<BlogOperationResult> CreateAsync(CreateBlogRequest request)
    {
        var metadataValidation = await ValidateMetadataAsync(request);
        if (metadataValidation is not null)
        {
            return BlogOperationResult.Failure(metadataValidation);
        }

        var hasImage = request.FeaturedImageLength is > 0 && request.FeaturedImageContent is not null;
        (string? Error, string? Extension) imageValidation = (null, null);
        if (hasImage)
        {
            imageValidation = await ValidateImageAsync(
                request.FeaturedImageOriginalFileName!, request.FeaturedImageContentType!,
                request.FeaturedImageLength!.Value, request.FeaturedImageContent!);
            if (imageValidation.Error is not null)
            {
                return BlogOperationResult.Failure(imageValidation.Error);
            }
        }
        var hasSecondaryImage = request.SecondaryImageLength is > 0 && request.SecondaryImageContent is not null;
        (string? Error, string? Extension) secondaryImageValidation = (null, null);
        if (hasSecondaryImage)
        {
            secondaryImageValidation = await ValidateImageAsync(
                request.SecondaryImageOriginalFileName!, request.SecondaryImageContentType!,
                request.SecondaryImageLength!.Value, request.SecondaryImageContent!);
            if (secondaryImageValidation.Error is not null)
            {
                return BlogOperationResult.Failure(secondaryImageValidation.Error);
            }
        }

        var blog = new Blog(request.BlogCategoryId, NullIfBlank(request.Author), request.PublishDate, request.Status, request.IsTrend);
        await _blogRepository.AddAsync(blog);

        // Yeni Blog'un Id'si SaveChanges'ten önce bilinmiyor (Translation.EntityId polimorfik +
        // görsel klasör anahtarı Id bazlı), bu yüzden Create'te iki SaveChanges çağrısı gerekir.
        await _unitOfWork.SaveChangesAsync();

        if (hasImage)
        {
            var fileName = $"{Guid.NewGuid():N}{imageValidation.Extension}";
            var folder = $"blog/{blog.Id}";

            string savedPath;
            try
            {
                savedPath = await _fileStorageService.SaveAsync(folder, request.FeaturedImageContent!, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blog kapak görseli diske kaydedilirken hata oluştu. BlogId={BlogId}", blog.Id);
                return BlogOperationResult.Failure("Kapak görseli kaydedilirken bir hata oluştu.");
            }

            blog.SetFeaturedImagePath(savedPath);
        }
        if (hasSecondaryImage)
        {
            var fileName = $"{Guid.NewGuid():N}{secondaryImageValidation.Extension}";
            var folder = $"blog/{blog.Id}";
            try
            {
                blog.SetSecondaryImagePath(await _fileStorageService.SaveAsync(folder, request.SecondaryImageContent!, fileName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blog ikinci görseli kaydedilirken hata oluştu. BlogId={BlogId}", blog.Id);
                return BlogOperationResult.Failure("İkinci görsel kaydedilirken bir hata oluştu.");
            }
        }

        await SaveTranslationsAsync(blog.Id, request.Translations);
        var tagIds = await ResolveTagIdsAsync(request.TagNames);
        await _blogRepository.ReplaceTagsAsync(blog.Id, tagIds);
        await _blogRepository.ReplaceRelatedPostsAsync(blog.Id, request.RelatedBlogIds.Distinct().ToList());
        await _unitOfWork.SaveChangesAsync();

        return BlogOperationResult.Success();
    }

    public async Task<BlogOperationResult> UpdateAsync(int id, UpdateBlogRequest request)
    {
        var blog = await _blogRepository.GetByIdAsync(id);
        if (blog is null)
        {
            return BlogOperationResult.Failure("Blog yazısı bulunamadı.");
        }

        var metadataValidation = await ValidateMetadataAsync(request);
        if (metadataValidation is not null)
        {
            return BlogOperationResult.Failure(metadataValidation);
        }

        var replacingImage = request.FeaturedImageLength is > 0 && request.FeaturedImageContent is not null;
        (string? Error, string? Extension) imageValidation = (null, null);
        if (replacingImage)
        {
            imageValidation = await ValidateImageAsync(
                request.FeaturedImageOriginalFileName!, request.FeaturedImageContentType!,
                request.FeaturedImageLength!.Value, request.FeaturedImageContent!);
            if (imageValidation.Error is not null)
            {
                return BlogOperationResult.Failure(imageValidation.Error);
            }
        }
        var replacingSecondaryImage = request.SecondaryImageLength is > 0 && request.SecondaryImageContent is not null;
        (string? Error, string? Extension) secondaryImageValidation = (null, null);
        if (replacingSecondaryImage)
        {
            secondaryImageValidation = await ValidateImageAsync(
                request.SecondaryImageOriginalFileName!, request.SecondaryImageContentType!,
                request.SecondaryImageLength!.Value, request.SecondaryImageContent!);
            if (secondaryImageValidation.Error is not null)
            {
                return BlogOperationResult.Failure(secondaryImageValidation.Error);
            }
        }

        var oldImagePath = blog.FeaturedImagePath;
        var oldSecondaryImagePath = blog.SecondaryImagePath;
        string? newSavedPath = null;
        string? newSecondarySavedPath = null;

        if (replacingImage)
        {
            var fileName = $"{Guid.NewGuid():N}{imageValidation.Extension}";
            var folder = $"blog/{blog.Id}";

            try
            {
                newSavedPath = await _fileStorageService.SaveAsync(folder, request.FeaturedImageContent!, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blog kapak görseli değiştirilirken diske kaydetme hatası oluştu. BlogId={BlogId}", id);
                return BlogOperationResult.Failure("Kapak görseli kaydedilirken bir hata oluştu.");
            }
        }
        if (replacingSecondaryImage)
        {
            var fileName = $"{Guid.NewGuid():N}{secondaryImageValidation.Extension}";
            var folder = $"blog/{blog.Id}";
            try
            {
                newSecondarySavedPath = await _fileStorageService.SaveAsync(folder, request.SecondaryImageContent!, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blog ikinci görseli değiştirilirken hata oluştu. BlogId={BlogId}", id);
                return BlogOperationResult.Failure("İkinci görsel kaydedilirken bir hata oluştu.");
            }
        }

        blog.UpdateDetails(request.BlogCategoryId, NullIfBlank(request.Author), request.PublishDate, request.Status, request.IsTrend);

        if (replacingImage)
        {
            blog.SetFeaturedImagePath(newSavedPath);
        }
        else if (request.RemoveFeaturedImage)
        {
            blog.SetFeaturedImagePath(null);
        }
        if (replacingSecondaryImage)
        {
            blog.SetSecondaryImagePath(newSecondarySavedPath);
        }
        else if (request.RemoveSecondaryImage)
        {
            blog.SetSecondaryImagePath(null);
        }

        await SaveTranslationsAsync(blog.Id, request.Translations);
        var tagIds = await ResolveTagIdsAsync(request.TagNames);
        await _blogRepository.ReplaceTagsAsync(blog.Id, tagIds);
        await _blogRepository.ReplaceRelatedPostsAsync(blog.Id, request.RelatedBlogIds.Distinct().ToList());
        await _unitOfWork.SaveChangesAsync();

        if ((replacingImage || request.RemoveFeaturedImage) && oldImagePath is not null)
        {
            try
            {
                _fileStorageService.Delete(oldImagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eski blog kapak görseli silinemedi. FilePath={FilePath}", oldImagePath);
            }
        }
        if ((replacingSecondaryImage || request.RemoveSecondaryImage) && oldSecondaryImagePath is not null)
        {
            try
            {
                _fileStorageService.Delete(oldSecondaryImagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eski blog ikinci görseli silinemedi. FilePath={FilePath}", oldSecondaryImagePath);
            }
        }

        return BlogOperationResult.Success();
    }

    public async Task<BlogOperationResult> DeleteAsync(int id)
    {
        var blog = await _blogRepository.GetByIdAsync(id);
        if (blog is null)
        {
            return BlogOperationResult.Failure("Blog yazısı bulunamadı.");
        }

        var imagePath = blog.FeaturedImagePath;
        var secondaryImagePath = blog.SecondaryImagePath;

        // BlogTag ve bu blog'un kendi RelatedPost satırları (BlogId tarafı) DB'de Cascade FK ile
        // otomatik silinir. RelatedBlogId tarafı Restrict olduğu için (bkz. BlogRelatedPostConfiguration)
        // bu blog'a başka yazılardan yapılan referanslar elle temizlenir.
        await _blogRepository.RemoveRelatedPostReferencesAsync(id);
        await _translationService.DeleteTranslationsForAsync(EntityType.Blog, id);
        _blogRepository.Remove(blog);

        await _unitOfWork.SaveChangesAsync();

        if (imagePath is not null)
        {
            try
            {
                _fileStorageService.Delete(imagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blog veritabanından silindi ama kapak görseli silinemedi. FilePath={FilePath}", imagePath);
            }
        }
        if (secondaryImagePath is not null)
        {
            try
            {
                _fileStorageService.Delete(secondaryImagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blog silindi ancak ikinci görsel silinemedi. FilePath={FilePath}", secondaryImagePath);
            }
        }

        return BlogOperationResult.Success();
    }

    private async Task<string?> ValidateMetadataAsync(BlogRequestBase request)
    {
        if (request.BlogCategoryId.HasValue)
        {
            var category = await _blogCategoryRepository.GetByIdAsync(request.BlogCategoryId.Value);
            if (category is null)
            {
                return "Seçilen blog kategorisi bulunamadı.";
            }
        }

        var activeLanguages = await _translationService.GetActiveLanguagesAsync();
        var trLanguage = activeLanguages.FirstOrDefault(l => l.Code == TrLanguageCode);

        if (trLanguage is null)
        {
            return "Aktif Türkçe (TR) dili bulunamadı.";
        }

        var trInput = request.Translations.FirstOrDefault(t => t.LanguageId == trLanguage.Id);
        if (trInput is null || string.IsNullOrWhiteSpace(trInput.Title))
        {
            return "Türkçe (TR) blog başlığı zorunludur.";
        }

        return null;
    }

    private async Task<(string? Error, string? Extension)> ValidateImageAsync(
        string originalFileName, string contentType, long length, Stream content)
    {
        if (length <= 0)
        {
            return ("Boş dosya yüklenemez.", null);
        }

        if (length > MaxImageSizeBytes)
        {
            return ($"Dosya boyutu {MaxImageSizeBytes / (1024 * 1024)} MB sınırını aşıyor.", null);
        }

        var extension = Path.GetExtension(originalFileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensionContentTypes.TryGetValue(extension, out var expectedContentType))
        {
            return ("İzin verilmeyen dosya uzantısı. Yalnızca .jpg, .jpeg, .png, .webp kabul edilir.", null);
        }

        if (!string.Equals(contentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
        {
            return ("Dosya uzantısı ile içerik tipi (MIME) uyuşmuyor.", null);
        }

        if (!content.CanSeek)
        {
            return ("Dosya akışı okunamadı.", null);
        }

        var header = new byte[12];
        content.Position = 0;
        var bytesRead = await content.ReadAsync(header.AsMemory(0, header.Length));
        content.Position = 0;

        if (!HasValidImageSignature(header, bytesRead, extension))
        {
            return ("Dosya içeriği belirtilen görsel formatıyla uyuşmuyor.", null);
        }

        return (null, extension);
    }

    private static bool HasValidImageSignature(byte[] header, int bytesRead, string extension)
    {
        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                return bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            case ".png":
                return bytesRead >= 8 &&
                    header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                    header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;
            case ".webp":
                return bytesRead >= 12 &&
                    header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F' &&
                    header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P';
            default:
                return false;
        }
    }

    private async Task<IReadOnlyList<int>> ResolveTagIdsAsync(IReadOnlyList<string> tagNames)
    {
        var ids = new List<int>();

        foreach (var rawName in tagNames)
        {
            var name = rawName?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var existing = await _tagRepository.GetByNameAsync(name);
            if (existing is not null)
            {
                if (!ids.Contains(existing.Id))
                {
                    ids.Add(existing.Id);
                }

                continue;
            }

            var tag = new Tag(name);
            await _tagRepository.AddAsync(tag);
            await _unitOfWork.SaveChangesAsync();
            ids.Add(tag.Id);
        }

        return ids;
    }

    private async Task SaveTranslationsAsync(int blogId, IReadOnlyList<BlogTranslationInput> translations)
    {
        foreach (var input in translations)
        {
            await SaveFieldAsync(blogId, input.LanguageId, BlogFields.Title, input.Title);
            await SaveFieldAsync(blogId, input.LanguageId, BlogFields.Excerpt, input.Excerpt);
            await SaveFieldAsync(blogId, input.LanguageId, BlogFields.Content, input.Content);
            await SaveFieldAsync(blogId, input.LanguageId, BlogFields.SeoUrl, input.SeoUrl);
            await SaveFieldAsync(blogId, input.LanguageId, BlogFields.MetaTitle, input.MetaTitle);
            await SaveFieldAsync(blogId, input.LanguageId, BlogFields.MetaDescription, input.MetaDescription);
        }
    }

    private async Task SaveFieldAsync(int blogId, int languageId, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            await _translationService.DeleteTranslationFieldAsync(EntityType.Blog, blogId, languageId, fieldName);
        }
        else
        {
            await _translationService.SetTranslationAsync(EntityType.Blog, blogId, languageId, fieldName, value.Trim());
        }
    }

    private async Task<BlogDto> MapToDtoAsync(Blog blog)
    {
        var translations = await _translationService.GetTranslationsAsync(EntityType.Blog, blog.Id);
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();

        var translationDtos = activeLanguages
            .Select(language =>
            {
                string? Get(string fieldName) => translations
                    .FirstOrDefault(t => t.LanguageId == language.Id && t.FieldName == fieldName)?.Value;

                return new BlogTranslationDto
                {
                    LanguageId = language.Id,
                    LanguageCode = language.Code,
                    LanguageName = language.Name,
                    Title = Get(BlogFields.Title),
                    Excerpt = Get(BlogFields.Excerpt),
                    Content = Get(BlogFields.Content),
                    SeoUrl = Get(BlogFields.SeoUrl),
                    MetaTitle = Get(BlogFields.MetaTitle),
                    MetaDescription = Get(BlogFields.MetaDescription)
                };
            })
            .ToList();

        string? categoryName = null;
        if (blog.BlogCategoryId.HasValue)
        {
            var categoryTranslations = await _translationService.GetTranslationsAsync(EntityType.BlogCategory, blog.BlogCategoryId.Value);
            categoryName = categoryTranslations
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == BlogCategoryFields.Name)?.Value;
        }

        var tags = await _blogRepository.GetTagsAsync(blog.Id);

        var relatedBlogIds = await _blogRepository.GetRelatedBlogIdsAsync(blog.Id);
        var relatedPosts = new List<BlogRelatedPostSummaryDto>();
        foreach (var relatedBlogId in relatedBlogIds)
        {
            var relatedBlog = await _blogRepository.GetByIdAsync(relatedBlogId);
            if (relatedBlog is null)
            {
                continue;
            }

            var relatedTranslations = await _translationService.GetTranslationsAsync(EntityType.Blog, relatedBlogId);
            var relatedTitle = relatedTranslations
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == BlogFields.Title)?.Value;
            var relatedSeoUrl = relatedTranslations
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == BlogFields.SeoUrl)?.Value;

            relatedPosts.Add(new BlogRelatedPostSummaryDto
            {
                Id = relatedBlog.Id,
                Title = relatedTitle ?? $"#{relatedBlog.Id}",
                SeoUrl = relatedSeoUrl,
                FeaturedImagePath = relatedBlog.FeaturedImagePath
            });
        }

        return new BlogDto
        {
            Id = blog.Id,
            BlogCategoryId = blog.BlogCategoryId,
            BlogCategoryName = categoryName,
            Author = blog.Author,
            PublishDate = blog.PublishDate,
            Status = blog.Status,
            IsTrend = blog.IsTrend,
            FeaturedImagePath = blog.FeaturedImagePath,
            SecondaryImagePath = blog.SecondaryImagePath,
            Tags = tags.Select(t => t.Name).OrderBy(n => n).ToList(),
            RelatedPosts = relatedPosts,
            Translations = translationDtos
        };
    }

    public async Task<IReadOnlyList<(int Id, string Label)>> GetBlogOptionsAsync(int? excludeId)
    {
        var blogs = await _blogRepository.GetAllAsync();
        var result = new List<(int Id, string Label)>();

        foreach (var blog in blogs)
        {
            if (blog.Id == excludeId)
            {
                continue;
            }

            var translations = await _translationService.GetTranslationsAsync(EntityType.Blog, blog.Id);
            var title = translations
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == BlogFields.Title)?.Value;

            result.Add((blog.Id, title ?? $"#{blog.Id}"));
        }

        return result;
    }

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
