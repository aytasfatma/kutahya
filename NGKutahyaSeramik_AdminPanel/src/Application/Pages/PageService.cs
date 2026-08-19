using Application.Translations;
using Domain.Entities;
using Domain.Enums;

namespace Application.Pages;

/// <summary>
/// Madde 16.2/17.2 — Sayfa Yönetimi (kurumsal sayfalar). Category/Collection'ın Translation-CRUD
/// deseniyle birebir, ama Page'in kendi native alanı yok (Title/SeoUrl/MetaTitle/MetaDescription
/// tamamen Translation'da) — yalnızca CreatedAt/UpdatedAt otomatik alanları var. IsActive/Status/
/// PublishDate/DisplayOrder/ParentId bilinçli olarak eklenmedi (doküman hiçbirini Page için istemiyor).
/// İçerik bloklarının (PageContentBlock) yönetimi ayrı PageContentBlockService'te.
/// </summary>
public class PageService
{
    private const string TrLanguageCode = "TR";

    private readonly IPageRepository _pageRepository;
    private readonly ITranslationService _translationService;
    private readonly PageContentBlockService _pageContentBlockService;
    private readonly IUnitOfWork _unitOfWork;

    public PageService(
        IPageRepository pageRepository,
        ITranslationService translationService,
        PageContentBlockService pageContentBlockService,
        IUnitOfWork unitOfWork)
    {
        _pageRepository = pageRepository;
        _translationService = translationService;
        _pageContentBlockService = pageContentBlockService;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<LanguageInfo>> GetActiveLanguagesAsync() =>
        _translationService.GetActiveLanguagesAsync();

    public async Task<IReadOnlyList<PageDto>> GetAllAsync()
    {
        var pages = await _pageRepository.GetAllAsync();

        var result = new List<PageDto>();
        foreach (var page in pages)
        {
            result.Add(await MapToDtoAsync(page));
        }

        return result;
    }

    public async Task<PageDto?> GetByIdAsync(int id)
    {
        var page = await _pageRepository.GetByIdAsync(id);
        return page is null ? null : await MapToDtoAsync(page);
    }

    public async Task<PageOperationResult> CreateAsync(CreatePageRequest request)
    {
        var validationError = await ValidateAsync(request.Translations);
        if (validationError is not null)
        {
            return PageOperationResult.Failure(validationError);
        }

        var page = new Page();
        await _pageRepository.AddAsync(page);

        // Yeni Page'in Id'si SaveChanges'ten önce bilinmiyor (Translation.EntityId polimorfik).
        await _unitOfWork.SaveChangesAsync();

        await SaveTranslationsAsync(page.Id, request.Translations);
        await _unitOfWork.SaveChangesAsync();

        return PageOperationResult.Success();
    }

    public async Task<PageOperationResult> UpdateAsync(int id, UpdatePageRequest request)
    {
        var page = await _pageRepository.GetByIdAsync(id);
        if (page is null)
        {
            return PageOperationResult.Failure("Sayfa bulunamadı.");
        }

        var validationError = await ValidateAsync(request.Translations);
        if (validationError is not null)
        {
            return PageOperationResult.Failure(validationError);
        }

        page.Touch();
        await SaveTranslationsAsync(page.Id, request.Translations);
        await _unitOfWork.SaveChangesAsync();

        return PageOperationResult.Success();
    }

    public async Task<PageOperationResult> DeleteAsync(int id)
    {
        var page = await _pageRepository.GetByIdAsync(id);
        if (page is null)
        {
            return PageOperationResult.Failure("Sayfa bulunamadı.");
        }

        // İçerik bloklarının DB kaydı + fiziksel görselleri, Page silinmeden önce açıkça temizlenir
        // (DB'de Cascade FK zaten satırları silecektir, ama fiziksel dosya temizliği garanti edilmeli
        // — ReferenceProjectImageService.DeleteAllForReferenceProjectAsync ile aynı desen).
        await _pageContentBlockService.DeleteAllForPageAsync(id);
        await _translationService.DeleteTranslationsForAsync(EntityType.Page, id);
        _pageRepository.Remove(page);

        await _unitOfWork.SaveChangesAsync();

        return PageOperationResult.Success();
    }

    private async Task<string?> ValidateAsync(IReadOnlyList<PageTranslationInput> translations)
    {
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();
        var trLanguage = activeLanguages.FirstOrDefault(l => l.Code == TrLanguageCode);

        if (trLanguage is null)
        {
            return "Aktif Türkçe (TR) dili bulunamadı.";
        }

        var trInput = translations.FirstOrDefault(t => t.LanguageId == trLanguage.Id);
        if (trInput is null || string.IsNullOrWhiteSpace(trInput.Title))
        {
            return "Türkçe (TR) sayfa başlığı zorunludur.";
        }

        return null;
    }

    private async Task SaveTranslationsAsync(int pageId, IReadOnlyList<PageTranslationInput> translations)
    {
        foreach (var input in translations)
        {
            await SaveFieldAsync(pageId, input.LanguageId, PageFields.Title, input.Title);
            await SaveFieldAsync(pageId, input.LanguageId, PageFields.SeoUrl, input.SeoUrl);
            await SaveFieldAsync(pageId, input.LanguageId, PageFields.MetaTitle, input.MetaTitle);
            await SaveFieldAsync(pageId, input.LanguageId, PageFields.MetaDescription, input.MetaDescription);
        }
    }

    private async Task SaveFieldAsync(int pageId, int languageId, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            await _translationService.DeleteTranslationFieldAsync(EntityType.Page, pageId, languageId, fieldName);
        }
        else
        {
            await _translationService.SetTranslationAsync(EntityType.Page, pageId, languageId, fieldName, value.Trim());
        }
    }

    private async Task<PageDto> MapToDtoAsync(Page page)
    {
        var translations = await _translationService.GetTranslationsAsync(EntityType.Page, page.Id);
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();

        var translationDtos = activeLanguages
            .Select(language =>
            {
                string? Get(string fieldName) => translations
                    .FirstOrDefault(t => t.LanguageId == language.Id && t.FieldName == fieldName)?.Value;

                return new PageTranslationDto
                {
                    LanguageId = language.Id,
                    LanguageCode = language.Code,
                    LanguageName = language.Name,
                    Title = Get(PageFields.Title),
                    SeoUrl = Get(PageFields.SeoUrl),
                    MetaTitle = Get(PageFields.MetaTitle),
                    MetaDescription = Get(PageFields.MetaDescription)
                };
            })
            .ToList();

        return new PageDto
        {
            Id = page.Id,
            CreatedAt = page.CreatedAt,
            UpdatedAt = page.UpdatedAt,
            Translations = translationDtos
        };
    }
}
