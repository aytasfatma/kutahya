using Application.Translations;
using Application.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.Blogs;

public class BlogCategoryService
{
    private const string TrLanguageCode = "TR";

    private readonly IBlogCategoryRepository _blogCategoryRepository;
    private readonly ITranslationService _translationService;
    private readonly IUnitOfWork _unitOfWork;

    public BlogCategoryService(
        IBlogCategoryRepository blogCategoryRepository,
        ITranslationService translationService,
        IUnitOfWork unitOfWork)
    {
        _blogCategoryRepository = blogCategoryRepository;
        _translationService = translationService;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync() =>
        _translationService.GetActiveLanguagesAsync();

    public async Task<int> GetNextDisplayOrderAsync()
    {
        var categories = await _blogCategoryRepository.GetAllAsync();
        return SortOrderValidation.Next(categories, c => c.DisplayOrder);
    }

    public async Task<IReadOnlyList<BlogCategoryDto>> GetAllAsync()
    {
        var categories = await _blogCategoryRepository.GetAllAsync();

        var result = new List<BlogCategoryDto>();
        foreach (var category in categories)
        {
            result.Add(await MapToDtoAsync(category));
        }

        return result;
    }

    public async Task<BlogCategoryDto?> GetByIdAsync(int id)
    {
        var category = await _blogCategoryRepository.GetByIdAsync(id);
        return category is null ? null : await MapToDtoAsync(category);
    }

    public async Task<BlogCategoryOperationResult> CreateAsync(CreateBlogCategoryRequest request)
    {
        var validation = await ValidateAsync(request.DisplayOrder, request.Translations, excludeCategoryId: null);
        if (!validation.Succeeded)
        {
            return validation;
        }

        var category = new BlogCategory(request.DisplayOrder);
        await _blogCategoryRepository.AddAsync(category);

        await _unitOfWork.SaveChangesAsync();

        await SaveTranslationsAsync(category.Id, request.Translations);
        await _unitOfWork.SaveChangesAsync();

        return BlogCategoryOperationResult.Success();
    }

    public async Task<BlogCategoryOperationResult> UpdateAsync(int id, UpdateBlogCategoryRequest request)
    {
        var category = await _blogCategoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return BlogCategoryOperationResult.Failure("Blog kategorisi bulunamadı.");
        }

        var validation = await ValidateAsync(request.DisplayOrder, request.Translations, excludeCategoryId: id);
        if (!validation.Succeeded)
        {
            return validation;
        }

        category.UpdateDetails(request.DisplayOrder);

        await SaveTranslationsAsync(category.Id, request.Translations);
        await _unitOfWork.SaveChangesAsync();

        return BlogCategoryOperationResult.Success();
    }

    public async Task<BlogCategoryOperationResult> ToggleActiveAsync(int id)
    {
        var category = await _blogCategoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return BlogCategoryOperationResult.Failure("Blog kategorisi bulunamadı.");
        }

        if (category.IsActive)
        {
            category.Deactivate();
        }
        else
        {
            category.Activate();
        }

        await _unitOfWork.SaveChangesAsync();
        return BlogCategoryOperationResult.Success();
    }

    public async Task<BlogCategoryOperationResult> DeleteAsync(int id)
    {
        var category = await _blogCategoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return BlogCategoryOperationResult.Failure("Blog kategorisi bulunamadı.");
        }

        // Blog.BlogCategoryId nullable FK + SetNull (bkz. BlogConfiguration) — kullanımdaki bir
        // kategori silinirse ilişkili Blog yazıları etkilenmez, yalnızca kategorisiz kalır. Bu
        // yüzden Task 6/7'deki gibi bir "kullanımda, silinemez" guard'ı burada gerekmiyor.
        await _translationService.DeleteTranslationsForAsync(EntityType.BlogCategory, id);
        _blogCategoryRepository.Remove(category);

        await _unitOfWork.SaveChangesAsync();

        return BlogCategoryOperationResult.Success();
    }

    private async Task<BlogCategoryOperationResult> ValidateAsync(
        int displayOrder,
        IReadOnlyList<BlogCategoryTranslationInput> translations,
        int? excludeCategoryId)
    {
        if (displayOrder < 1)
        {
            return BlogCategoryOperationResult.Failure(SortOrderValidationMessages.Minimum);
        }

        var activeLanguages = await _translationService.GetActiveLanguagesAsync();
        var trLanguage = activeLanguages.FirstOrDefault(l => l.Code == TrLanguageCode);

        if (trLanguage is null)
        {
            return BlogCategoryOperationResult.Failure("Aktif Türkçe (TR) dili bulunamadı.");
        }

        var trInput = translations.FirstOrDefault(t => t.LanguageId == trLanguage.Id);
        if (trInput is null || string.IsNullOrWhiteSpace(trInput.Name))
        {
            return BlogCategoryOperationResult.Failure("Türkçe (TR) kategori adı zorunludur.");
        }

        var isDuplicate = await IsDuplicateNameAsync(trInput.Name.Trim(), excludeCategoryId);
        if (isDuplicate)
        {
            return BlogCategoryOperationResult.Failure("Bu isimde başka bir blog kategorisi zaten var.");
        }

        var allCategories = await _blogCategoryRepository.GetAllAsync();
        if (SortOrderValidation.HasDuplicate(allCategories, displayOrder, excludeCategoryId, c => c.Id, c => c.DisplayOrder))
        {
            return BlogCategoryOperationResult.Failure(SortOrderValidationMessages.Duplicate);
        }

        return BlogCategoryOperationResult.Success();
    }

    private async Task<bool> IsDuplicateNameAsync(string trName, int? excludeCategoryId)
    {
        var all = await _blogCategoryRepository.GetAllAsync();

        foreach (var category in all)
        {
            if (excludeCategoryId.HasValue && category.Id == excludeCategoryId.Value)
            {
                continue;
            }

            var translations = await _translationService.GetTranslationsAsync(EntityType.BlogCategory, category.Id);
            var name = translations
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == BlogCategoryFields.Name)?.Value;

            if (name is not null && string.Equals(name.Trim(), trName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private async Task SaveTranslationsAsync(int categoryId, IReadOnlyList<BlogCategoryTranslationInput> translations)
    {
        foreach (var input in translations)
        {
            await SaveFieldAsync(categoryId, input.LanguageId, BlogCategoryFields.Name, input.Name);
        }
    }

    private async Task SaveFieldAsync(int categoryId, int languageId, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            await _translationService.DeleteTranslationFieldAsync(EntityType.BlogCategory, categoryId, languageId, fieldName);
        }
        else
        {
            await _translationService.SetTranslationAsync(EntityType.BlogCategory, categoryId, languageId, fieldName, value.Trim());
        }
    }

    private async Task<BlogCategoryDto> MapToDtoAsync(BlogCategory category)
    {
        var translations = await _translationService.GetTranslationsAsync(EntityType.BlogCategory, category.Id);
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();

        var translationDtos = activeLanguages
            .Select(language =>
            {
                string? Get(string fieldName) => translations
                    .FirstOrDefault(t => t.LanguageId == language.Id && t.FieldName == fieldName)?.Value;

                return new BlogCategoryTranslationDto
                {
                    LanguageId = language.Id,
                    LanguageCode = language.Code,
                    LanguageName = language.Name,
                    Name = Get(BlogCategoryFields.Name)
                };
            })
            .ToList();

        return new BlogCategoryDto
        {
            Id = category.Id,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            Translations = translationDtos
        };
    }
}

