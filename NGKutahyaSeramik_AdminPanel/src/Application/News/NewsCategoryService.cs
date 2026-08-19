using Application.Translations;
using Application.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.News;

public class NewsCategoryService
{
    private const string TrLanguageCode = "TR";

    private readonly INewsCategoryRepository _newsCategoryRepository;
    private readonly ITranslationService _translationService;
    private readonly IUnitOfWork _unitOfWork;

    public NewsCategoryService(
        INewsCategoryRepository newsCategoryRepository,
        ITranslationService translationService,
        IUnitOfWork unitOfWork)
    {
        _newsCategoryRepository = newsCategoryRepository;
        _translationService = translationService;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync() =>
        _translationService.GetActiveLanguagesAsync();

    public async Task<int> GetNextDisplayOrderAsync()
    {
        var categories = await _newsCategoryRepository.GetAllAsync();
        return SortOrderValidation.Next(categories, c => c.DisplayOrder);
    }

    public async Task<IReadOnlyList<NewsCategoryDto>> GetAllAsync()
    {
        var categories = await _newsCategoryRepository.GetAllAsync();

        var result = new List<NewsCategoryDto>();
        foreach (var category in categories)
        {
            result.Add(await MapToDtoAsync(category));
        }

        return result;
    }

    public async Task<NewsCategoryDto?> GetByIdAsync(int id)
    {
        var category = await _newsCategoryRepository.GetByIdAsync(id);
        return category is null ? null : await MapToDtoAsync(category);
    }

    public async Task<NewsCategoryOperationResult> CreateAsync(CreateNewsCategoryRequest request)
    {
        var validation = await ValidateAsync(request.DisplayOrder, request.Translations, excludeCategoryId: null);
        if (!validation.Succeeded)
        {
            return validation;
        }

        var category = new NewsCategory(request.DisplayOrder);
        await _newsCategoryRepository.AddAsync(category);

        await _unitOfWork.SaveChangesAsync();

        await SaveTranslationsAsync(category.Id, request.Translations);
        await _unitOfWork.SaveChangesAsync();

        return NewsCategoryOperationResult.Success();
    }

    public async Task<NewsCategoryOperationResult> UpdateAsync(int id, UpdateNewsCategoryRequest request)
    {
        var category = await _newsCategoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return NewsCategoryOperationResult.Failure("Bülten kategorisi bulunamadı.");
        }

        var validation = await ValidateAsync(request.DisplayOrder, request.Translations, excludeCategoryId: id);
        if (!validation.Succeeded)
        {
            return validation;
        }

        category.UpdateDetails(request.DisplayOrder);

        await SaveTranslationsAsync(category.Id, request.Translations);
        await _unitOfWork.SaveChangesAsync();

        return NewsCategoryOperationResult.Success();
    }

    public async Task<NewsCategoryOperationResult> ToggleActiveAsync(int id)
    {
        var category = await _newsCategoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return NewsCategoryOperationResult.Failure("Bülten kategorisi bulunamadı.");
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
        return NewsCategoryOperationResult.Success();
    }

    public async Task<NewsCategoryOperationResult> DeleteAsync(int id)
    {
        var category = await _newsCategoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return NewsCategoryOperationResult.Failure("Bülten kategorisi bulunamadı.");
        }

        // News.NewsCategoryId nullable FK + SetNull (bkz. NewsConfiguration) — kullanımdaki bir
        // kategori silinirse ilişkili haberler etkilenmez, yalnızca kategorisiz kalır.
        await _translationService.DeleteTranslationsForAsync(EntityType.NewsCategory, id);
        _newsCategoryRepository.Remove(category);

        await _unitOfWork.SaveChangesAsync();

        return NewsCategoryOperationResult.Success();
    }

    private async Task<NewsCategoryOperationResult> ValidateAsync(
        int displayOrder,
        IReadOnlyList<NewsCategoryTranslationInput> translations,
        int? excludeCategoryId)
    {
        if (displayOrder < 1)
        {
            return NewsCategoryOperationResult.Failure(SortOrderValidationMessages.Minimum);
        }

        var activeLanguages = await _translationService.GetActiveLanguagesAsync();
        var trLanguage = activeLanguages.FirstOrDefault(l => l.Code == TrLanguageCode);

        if (trLanguage is null)
        {
            return NewsCategoryOperationResult.Failure("Aktif Türkçe (TR) dili bulunamadı.");
        }

        var trInput = translations.FirstOrDefault(t => t.LanguageId == trLanguage.Id);
        if (trInput is null || string.IsNullOrWhiteSpace(trInput.Name))
        {
            return NewsCategoryOperationResult.Failure("Türkçe (TR) kategori adı zorunludur.");
        }

        var isDuplicate = await IsDuplicateNameAsync(trInput.Name.Trim(), excludeCategoryId);
        if (isDuplicate)
        {
            return NewsCategoryOperationResult.Failure("Bu isimde başka bir haber kategorisi zaten var.");
        }

        var allCategories = await _newsCategoryRepository.GetAllAsync();
        if (SortOrderValidation.HasDuplicate(allCategories, displayOrder, excludeCategoryId, c => c.Id, c => c.DisplayOrder))
        {
            return NewsCategoryOperationResult.Failure(SortOrderValidationMessages.Duplicate);
        }

        return NewsCategoryOperationResult.Success();
    }

    private async Task<bool> IsDuplicateNameAsync(string trName, int? excludeCategoryId)
    {
        var all = await _newsCategoryRepository.GetAllAsync();

        foreach (var category in all)
        {
            if (excludeCategoryId.HasValue && category.Id == excludeCategoryId.Value)
            {
                continue;
            }

            var translations = await _translationService.GetTranslationsAsync(EntityType.NewsCategory, category.Id);
            var name = translations
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == NewsCategoryFields.Name)?.Value;

            if (name is not null && string.Equals(name.Trim(), trName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private async Task SaveTranslationsAsync(int categoryId, IReadOnlyList<NewsCategoryTranslationInput> translations)
    {
        foreach (var input in translations)
        {
            await SaveFieldAsync(categoryId, input.LanguageId, NewsCategoryFields.Name, input.Name);
        }
    }

    private async Task SaveFieldAsync(int categoryId, int languageId, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            await _translationService.DeleteTranslationFieldAsync(EntityType.NewsCategory, categoryId, languageId, fieldName);
        }
        else
        {
            await _translationService.SetTranslationAsync(EntityType.NewsCategory, categoryId, languageId, fieldName, value.Trim());
        }
    }

    private async Task<NewsCategoryDto> MapToDtoAsync(NewsCategory category)
    {
        var translations = await _translationService.GetTranslationsAsync(EntityType.NewsCategory, category.Id);
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();

        var translationDtos = activeLanguages
            .Select(language =>
            {
                string? Get(string fieldName) => translations
                    .FirstOrDefault(t => t.LanguageId == language.Id && t.FieldName == fieldName)?.Value;

                return new NewsCategoryTranslationDto
                {
                    LanguageId = language.Id,
                    LanguageCode = language.Code,
                    LanguageName = language.Name,
                    Name = Get(NewsCategoryFields.Name)
                };
            })
            .ToList();

        return new NewsCategoryDto
        {
            Id = category.Id,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            Translations = translationDtos
        };
    }
}

