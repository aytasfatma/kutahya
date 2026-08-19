using Application.Storage;
using Application.Translations;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.News;

/// <summary>
/// Madde 22 — Haberler Modülü. BlogService ile birebir aynı desen (tekil kapak görseli doğrudan
/// News.FeaturedImagePath'te, ProductImage ile aynı doğrulama/güvenlik), ancak Tags/Author/Excerpt
/// yok — doküman Haber için bunları hiç anmıyor, Blog'dan körü körüne kopyalanmadı.
/// </summary>
public class NewsService
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

    private readonly INewsRepository _newsRepository;
    private readonly INewsCategoryRepository _newsCategoryRepository;
    private readonly ITranslationService _translationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NewsService> _logger;

    public NewsService(
        INewsRepository newsRepository,
        INewsCategoryRepository newsCategoryRepository,
        ITranslationService translationService,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<NewsService> logger)
    {
        _newsRepository = newsRepository;
        _newsCategoryRepository = newsCategoryRepository;
        _translationService = translationService;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync() =>
        _translationService.GetActiveLanguagesAsync();

    public async Task<IReadOnlyList<(int Id, string Label)>> GetNewsCategoryOptionsAsync()
    {
        var categories = await _newsCategoryRepository.GetAllAsync();
        var result = new List<(int Id, string Label)>();

        foreach (var category in categories.OrderBy(c => c.DisplayOrder))
        {
            var translations = await _translationService.GetTranslationsAsync(EntityType.NewsCategory, category.Id);
            var name = translations
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == NewsCategoryFields.Name)?.Value;

            result.Add((category.Id, name ?? $"#{category.Id}"));
        }

        return result;
    }

    public async Task<IReadOnlyList<NewsDto>> GetAllAsync()
    {
        var newsItems = await _newsRepository.GetAllAsync();

        var result = new List<NewsDto>();
        foreach (var news in newsItems)
        {
            result.Add(await MapToDtoAsync(news));
        }

        return result;
    }

    public async Task<NewsDto?> GetByIdAsync(int id)
    {
        var news = await _newsRepository.GetByIdAsync(id);
        return news is null ? null : await MapToDtoAsync(news);
    }

    public async Task<NewsOperationResult> CreateAsync(CreateNewsRequest request)
    {
        var metadataValidation = await ValidateMetadataAsync(request);
        if (metadataValidation is not null)
        {
            return NewsOperationResult.Failure(metadataValidation);
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
                return NewsOperationResult.Failure(imageValidation.Error);
            }
        }

        var news = new Domain.Entities.News(request.NewsCategoryId, request.PublishDate, request.Status);
        await _newsRepository.AddAsync(news);

        // Yeni News'in Id'si SaveChanges'ten önce bilinmiyor (Translation.EntityId polimorfik +
        // görsel klasör anahtarı Id bazlı), bu yüzden Create'te iki SaveChanges çağrısı gerekir.
        await _unitOfWork.SaveChangesAsync();

        if (hasImage)
        {
            var fileName = $"{Guid.NewGuid():N}{imageValidation.Extension}";
            var folder = $"news/{news.Id}";

            string savedPath;
            try
            {
                savedPath = await _fileStorageService.SaveAsync(folder, request.FeaturedImageContent!, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Haber kapak görseli diske kaydedilirken hata oluştu. NewsId={NewsId}", news.Id);
                return NewsOperationResult.Failure("Kapak görseli kaydedilirken bir hata oluştu.");
            }

            news.SetFeaturedImagePath(savedPath);
        }

        await SaveTranslationsAsync(news.Id, request.Translations);
        await _newsRepository.ReplaceRelatedNewsAsync(news.Id, request.RelatedNewsIds.Distinct().ToList());
        await _unitOfWork.SaveChangesAsync();

        return NewsOperationResult.Success();
    }

    public async Task<NewsOperationResult> UpdateAsync(int id, UpdateNewsRequest request)
    {
        var news = await _newsRepository.GetByIdAsync(id);
        if (news is null)
        {
            return NewsOperationResult.Failure("Bülten bulunamadı.");
        }

        var metadataValidation = await ValidateMetadataAsync(request);
        if (metadataValidation is not null)
        {
            return NewsOperationResult.Failure(metadataValidation);
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
                return NewsOperationResult.Failure(imageValidation.Error);
            }
        }

        var oldImagePath = news.FeaturedImagePath;
        string? newSavedPath = null;

        if (replacingImage)
        {
            var fileName = $"{Guid.NewGuid():N}{imageValidation.Extension}";
            var folder = $"news/{news.Id}";

            try
            {
                newSavedPath = await _fileStorageService.SaveAsync(folder, request.FeaturedImageContent!, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Haber kapak görseli değiştirilirken diske kaydetme hatası oluştu. NewsId={NewsId}", id);
                return NewsOperationResult.Failure("Kapak görseli kaydedilirken bir hata oluştu.");
            }
        }

        news.UpdateDetails(request.NewsCategoryId, request.PublishDate, request.Status);

        if (replacingImage)
        {
            news.SetFeaturedImagePath(newSavedPath);
        }
        else if (request.RemoveFeaturedImage)
        {
            news.SetFeaturedImagePath(null);
        }

        await SaveTranslationsAsync(news.Id, request.Translations);
        await _newsRepository.ReplaceRelatedNewsAsync(news.Id, request.RelatedNewsIds.Distinct().ToList());
        await _unitOfWork.SaveChangesAsync();

        if ((replacingImage || request.RemoveFeaturedImage) && oldImagePath is not null)
        {
            try
            {
                _fileStorageService.Delete(oldImagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eski haber kapak görseli silinemedi. FilePath={FilePath}", oldImagePath);
            }
        }

        return NewsOperationResult.Success();
    }

    public async Task<NewsOperationResult> DeleteAsync(int id)
    {
        var news = await _newsRepository.GetByIdAsync(id);
        if (news is null)
        {
            return NewsOperationResult.Failure("Bülten bulunamadı.");
        }

        var imagePath = news.FeaturedImagePath;

        // NewsRelatedPosts'ta bu haberin kendi satırları (NewsId tarafı) DB'de Cascade FK ile otomatik
        // silinir. RelatedNewsId tarafı Restrict olduğu için (bkz. NewsRelatedPostConfiguration) bu
        // habere başka haberlerden yapılan referanslar elle temizlenir.
        await _newsRepository.RemoveRelatedPostReferencesAsync(id);
        await _translationService.DeleteTranslationsForAsync(EntityType.News, id);
        _newsRepository.Remove(news);

        await _unitOfWork.SaveChangesAsync();

        if (imagePath is not null)
        {
            try
            {
                _fileStorageService.Delete(imagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Haber veritabanından silindi ama kapak görseli silinemedi. FilePath={FilePath}", imagePath);
            }
        }

        return NewsOperationResult.Success();
    }

    private async Task<string?> ValidateMetadataAsync(NewsRequestBase request)
    {
        if (request.NewsCategoryId.HasValue)
        {
            var category = await _newsCategoryRepository.GetByIdAsync(request.NewsCategoryId.Value);
            if (category is null)
            {
                return "Seçilen haber kategorisi bulunamadı.";
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
            return "Türkçe (TR) haber başlığı zorunludur.";
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

    private async Task SaveTranslationsAsync(int newsId, IReadOnlyList<NewsTranslationInput> translations)
    {
        foreach (var input in translations)
        {
            await SaveFieldAsync(newsId, input.LanguageId, NewsFields.Title, input.Title);
            await SaveFieldAsync(newsId, input.LanguageId, NewsFields.Excerpt, input.Excerpt);
            await SaveFieldAsync(newsId, input.LanguageId, NewsFields.Content, input.Content);
            await SaveFieldAsync(newsId, input.LanguageId, NewsFields.SeoUrl, input.SeoUrl);
            await SaveFieldAsync(newsId, input.LanguageId, NewsFields.MetaTitle, input.MetaTitle);
            await SaveFieldAsync(newsId, input.LanguageId, NewsFields.MetaDescription, input.MetaDescription);
        }
    }

    private async Task SaveFieldAsync(int newsId, int languageId, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            await _translationService.DeleteTranslationFieldAsync(EntityType.News, newsId, languageId, fieldName);
        }
        else
        {
            await _translationService.SetTranslationAsync(EntityType.News, newsId, languageId, fieldName, value.Trim());
        }
    }

    private async Task<NewsDto> MapToDtoAsync(Domain.Entities.News news)
    {
        var translations = await _translationService.GetTranslationsAsync(EntityType.News, news.Id);
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();

        var translationDtos = activeLanguages
            .Select(language =>
            {
                string? Get(string fieldName) => translations
                    .FirstOrDefault(t => t.LanguageId == language.Id && t.FieldName == fieldName)?.Value;

                return new NewsTranslationDto
                {
                    LanguageId = language.Id,
                    LanguageCode = language.Code,
                    LanguageName = language.Name,
                    Title = Get(NewsFields.Title),
                    Excerpt = Get(NewsFields.Excerpt),
                    Content = Get(NewsFields.Content),
                    SeoUrl = Get(NewsFields.SeoUrl),
                    MetaTitle = Get(NewsFields.MetaTitle),
                    MetaDescription = Get(NewsFields.MetaDescription)
                };
            })
            .ToList();

        string? categoryName = null;
        if (news.NewsCategoryId.HasValue)
        {
            var categoryTranslations = await _translationService.GetTranslationsAsync(EntityType.NewsCategory, news.NewsCategoryId.Value);
            categoryName = categoryTranslations
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == NewsCategoryFields.Name)?.Value;
        }

        var relatedNewsIds = await _newsRepository.GetRelatedNewsIdsAsync(news.Id);
        var relatedPosts = new List<NewsRelatedPostSummaryDto>();
        foreach (var relatedNewsId in relatedNewsIds)
        {
            var relatedNews = await _newsRepository.GetByIdAsync(relatedNewsId);
            if (relatedNews is null)
            {
                continue;
            }

            var relatedTranslations = await _translationService.GetTranslationsAsync(EntityType.News, relatedNewsId);
            var relatedTitle = relatedTranslations
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == NewsFields.Title)?.Value;
            var relatedSeoUrl = relatedTranslations
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == NewsFields.SeoUrl)?.Value;

            relatedPosts.Add(new NewsRelatedPostSummaryDto
            {
                Id = relatedNews.Id,
                Title = relatedTitle ?? $"#{relatedNews.Id}",
                SeoUrl = relatedSeoUrl,
                FeaturedImagePath = relatedNews.FeaturedImagePath
            });
        }

        return new NewsDto
        {
            Id = news.Id,
            NewsCategoryId = news.NewsCategoryId,
            NewsCategoryName = categoryName,
            PublishDate = news.PublishDate,
            Status = news.Status,
            FeaturedImagePath = news.FeaturedImagePath,
            RelatedPosts = relatedPosts,
            Translations = translationDtos
        };
    }

    public async Task<IReadOnlyList<(int Id, string Label)>> GetNewsOptionsAsync(int? excludeId)
    {
        var newsItems = await _newsRepository.GetAllAsync();
        var result = new List<(int Id, string Label)>();

        foreach (var news in newsItems)
        {
            if (news.Id == excludeId)
            {
                continue;
            }

            var translations = await _translationService.GetTranslationsAsync(EntityType.News, news.Id);
            var title = translations
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == NewsFields.Title)?.Value;

            result.Add((news.Id, title ?? $"#{news.Id}"));
        }

        return result;
    }
}
