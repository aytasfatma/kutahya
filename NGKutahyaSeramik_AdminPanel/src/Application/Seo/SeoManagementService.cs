using Application.Blogs;
using Application.Categories;
using Application.Collections;
using Application.News;
using Application.Pages;
using Application.Products;
using Application.ReferenceProjects;
using Application.Translations;
using Domain.Enums;

namespace Application.Seo;

/// <summary>
/// Backlog #4 — SEO veri sözleşmesi + SEO Yönetimi. Mevcut Translation mimarisi (SeoUrl/MetaTitle/
/// MetaDescription alanları, ADR-007/012) TEK doğruluk kaynağı — ikinci bir SEO tablosu YOK, bu servis
/// yalnızca 7 modülün (Product/Category/Collection/Blog/News/Page/ReferenceProject) zaten var olan
/// servislerini (kayıt varlığı + görünen ad için) ve `ITranslationService`'i (asıl SEO alan okuma/
/// yazma için) sarmalayan ince bir cross-cutting katman. Banner ve Dealer bilinçli olarak dışarıda
/// bırakıldı — Banner'ın (BannerFields.cs) hiç SEO alanı yok (Task 10/ADR: Madde 17.2/36.1 Banner
/// için SEO hiç anmıyor), Dealer'ın SeoUrl'i yok (ADR-008: URL City/District'ten programatik türetiliyor).
/// ReferenceProject yalnızca SeoUrl'e sahip (ReferenceProjectFields.cs'te MetaTitle/MetaDescription
/// hiç yok) — bu servis bunu `SupportsMetaFields=false` ile açıkça yansıtıyor, icat etmiyor.
/// </summary>
public class SeoManagementService
{
    private static readonly IReadOnlyList<SeoContentTypeDto> SupportedTypes =
    [
        new() { EntityType = EntityType.Product, Label = "Ürün", SupportsMetaFields = true },
        new() { EntityType = EntityType.Blog, Label = "Blog", SupportsMetaFields = true },
        new() { EntityType = EntityType.News, Label = "Bülten", SupportsMetaFields = true },
        new() { EntityType = EntityType.Page, Label = "Sayfa", SupportsMetaFields = true },
        new() { EntityType = EntityType.ReferenceProject, Label = "Referans Proje", SupportsMetaFields = false }
    ];

    private readonly ProductService _productService;
    private readonly CategoryService _categoryService;
    private readonly CollectionService _collectionService;
    private readonly BlogService _blogService;
    private readonly NewsService _newsService;
    private readonly PageService _pageService;
    private readonly ReferenceProjectService _referenceProjectService;
    private readonly ITranslationService _translationService;
    private readonly IUnitOfWork _unitOfWork;

    public SeoManagementService(
        ProductService productService,
        CategoryService categoryService,
        CollectionService collectionService,
        BlogService blogService,
        NewsService newsService,
        PageService pageService,
        ReferenceProjectService referenceProjectService,
        ITranslationService translationService,
        IUnitOfWork unitOfWork)
    {
        _productService = productService;
        _categoryService = categoryService;
        _collectionService = collectionService;
        _blogService = blogService;
        _newsService = newsService;
        _pageService = pageService;
        _referenceProjectService = referenceProjectService;
        _translationService = translationService;
        _unitOfWork = unitOfWork;
    }

    public IReadOnlyList<SeoContentTypeDto> GetSupportedContentTypes() => SupportedTypes;

    public bool IsSupported(EntityType entityType) => SupportedTypes.Any(t => t.EntityType == entityType);

    public bool SupportsMetaFields(EntityType entityType) =>
        SupportedTypes.FirstOrDefault(t => t.EntityType == entityType)?.SupportsMetaFields ?? false;

    public async Task<IReadOnlyList<SeoRecordSummaryDto>> GetRecordsAsync(EntityType entityType)
    {
        return entityType switch
        {
            EntityType.Product => (await _productService.GetAllAsync())
                .Select(p => new SeoRecordSummaryDto { EntityType = entityType, EntityId = p.Id, DisplayName = p.DisplayName ?? p.ProductCode })
                .ToList(),
            EntityType.Category => (await _categoryService.GetTreeAsync())
                .Select(c => new SeoRecordSummaryDto { EntityType = entityType, EntityId = c.Id, DisplayName = c.DisplayName ?? $"#{c.Id}" })
                .ToList(),
            EntityType.Collection => (await _collectionService.GetAllAsync())
                .Select(c => new SeoRecordSummaryDto { EntityType = entityType, EntityId = c.Id, DisplayName = c.DisplayName ?? $"#{c.Id}" })
                .ToList(),
            EntityType.Blog => (await _blogService.GetAllAsync())
                .Select(b => new SeoRecordSummaryDto { EntityType = entityType, EntityId = b.Id, DisplayName = b.DisplayTitle ?? $"#{b.Id}" })
                .ToList(),
            EntityType.News => (await _newsService.GetAllAsync())
                .Select(n => new SeoRecordSummaryDto { EntityType = entityType, EntityId = n.Id, DisplayName = n.DisplayTitle ?? $"#{n.Id}" })
                .ToList(),
            EntityType.Page => (await _pageService.GetAllAsync())
                .Select(p => new SeoRecordSummaryDto { EntityType = entityType, EntityId = p.Id, DisplayName = p.DisplayTitle ?? $"#{p.Id}" })
                .ToList(),
            EntityType.ReferenceProject => (await _referenceProjectService.GetAllAsync())
                .Select(r => new SeoRecordSummaryDto { EntityType = entityType, EntityId = r.Id, DisplayName = r.DisplayName ?? $"#{r.Id}" })
                .ToList(),
            _ => throw new InvalidOperationException($"'{entityType}' SEO Yönetimi tarafından desteklenmiyor.")
        };
    }

    public async Task<SeoRecordDetailDto?> GetRecordDetailAsync(EntityType entityType, int entityId)
    {
        var record = (await GetRecordsAsync(entityType)).FirstOrDefault(r => r.EntityId == entityId);
        if (record is null)
        {
            return null;
        }

        var supportsMeta = SupportsMetaFields(entityType);
        var translations = await _translationService.GetTranslationsAsync(entityType, entityId);
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();

        var seoUrlField = SeoUrlFieldName(entityType);
        var metaTitleField = MetaTitleFieldName(entityType);
        var metaDescriptionField = MetaDescriptionFieldName(entityType);

        var languages = activeLanguages.Select(language =>
        {
            string? Get(string? fieldName) => fieldName is null
                ? null
                : translations.FirstOrDefault(t => t.LanguageId == language.Id && t.FieldName == fieldName)?.Value;

            return new SeoLanguageFieldDto
            {
                LanguageId = language.Id,
                LanguageCode = language.Code,
                LanguageName = language.Name,
                SeoUrl = Get(seoUrlField),
                MetaTitle = supportsMeta ? Get(metaTitleField) : null,
                MetaDescription = supportsMeta ? Get(metaDescriptionField) : null
            };
        }).ToList();

        return new SeoRecordDetailDto
        {
            EntityType = entityType,
            EntityId = entityId,
            DisplayName = record.DisplayName,
            SupportsMetaFields = supportsMeta,
            Languages = languages
        };
    }

    /// <summary>Bir kayıt+dil için SEO alanlarını günceller. `MetaTitle`/`MetaDescription`, tip
    /// bunları desteklemiyorsa (ReferenceProject) sessizce yok sayılır — hata değil, çünkü View bu
    /// alanları zaten göstermiyor (dokümanda/entity'de olmayan alan icat edilmedi).</summary>
    public async Task<(bool Succeeded, string? ErrorMessage, bool DuplicateWarning)> UpdateAsync(
        EntityType entityType, int entityId, int languageId, string? seoUrl, string? metaTitle, string? metaDescription)
    {
        if (!IsSupported(entityType))
        {
            return (false, $"'{entityType}' SEO Yönetimi tarafından desteklenmiyor.", false);
        }

        var record = (await GetRecordsAsync(entityType)).FirstOrDefault(r => r.EntityId == entityId);
        if (record is null)
        {
            return (false, "Kayıt bulunamadı.", false);
        }

        // EntityType + Dil doğrulaması: entityType zaten yukarıda kontrol edildi, languageId de aynı
        // titizlikte doğrulanmalı — aksi halde ya geçersiz bir LanguageId FK ihlaliyle (DbUpdateException,
        // 500) SaveChangesAsync'te patlar ya da pasif bir dile sessizce yazılır (ProductService.ValidateAsync'in
        // "aktif TR bulunamadı" kontrolüyle aynı ilke, buraya da uygulandı).
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();
        if (activeLanguages.All(l => l.Id != languageId))
        {
            return (false, "Seçilen dil aktif değil veya bulunamadı.", false);
        }

        var seoUrlField = SeoUrlFieldName(entityType);
        await SetOrDeleteAsync(entityType, entityId, languageId, seoUrlField, seoUrl);

        if (SupportsMetaFields(entityType))
        {
            await SetOrDeleteAsync(entityType, entityId, languageId, MetaTitleFieldName(entityType)!, metaTitle);
            await SetOrDeleteAsync(entityType, entityId, languageId, MetaDescriptionFieldName(entityType)!, metaDescription);
        }

        await _unitOfWork.SaveChangesAsync();

        var duplicateWarning = false;
        if (!string.IsNullOrWhiteSpace(seoUrl))
        {
            var normalized = SeoUrlNormalizer.Normalize(seoUrl);
            var duplicates = await GetDuplicateSeoUrlReportAsync();
            duplicateWarning = duplicates.Any(g =>
                g.EntityType == entityType &&
                g.NormalizedSeoUrl == normalized &&
                g.Records.Any(r => r.EntityId == entityId) &&
                g.Records.Count > 1);
        }

        return (true, null, duplicateWarning);
    }

    /// <summary>"Eksik SEO alanlarını listeleme" — tüm desteklenen tipler × tüm aktif diller
    /// taranarak SeoUrl (ve destekleniyorsa MetaTitle/MetaDescription) boş olan satırlar toplanır.</summary>
    public async Task<IReadOnlyList<SeoMissingFieldDto>> GetMissingSeoReportAsync()
    {
        var result = new List<SeoMissingFieldDto>();

        foreach (var type in SupportedTypes)
        {
            var records = await GetRecordsAsync(type.EntityType);
            foreach (var record in records)
            {
                var detail = await GetRecordDetailAsync(type.EntityType, record.EntityId);
                if (detail is null)
                {
                    continue;
                }

                foreach (var language in detail.Languages)
                {
                    var missing = new List<string>();
                    if (string.IsNullOrWhiteSpace(language.SeoUrl))
                    {
                        missing.Add("SeoUrl");
                    }

                    if (detail.SupportsMetaFields)
                    {
                        if (string.IsNullOrWhiteSpace(language.MetaTitle))
                        {
                            missing.Add("MetaTitle");
                        }

                        if (string.IsNullOrWhiteSpace(language.MetaDescription))
                        {
                            missing.Add("MetaDescription");
                        }
                    }

                    if (missing.Count > 0)
                    {
                        result.Add(new SeoMissingFieldDto
                        {
                            EntityType = type.EntityType,
                            EntityId = record.EntityId,
                            DisplayName = record.DisplayName,
                            LanguageCode = language.LanguageCode,
                            MissingFields = missing
                        });
                    }
                }
            }
        }

        return result;
    }

    /// <summary>"Duplicate SEO URL kontrolü" — kullanıcının onayladığı kapsam: EntityType+Dil bazında
    /// (Product için Kategori-segmentli gerçek public URL ile birebir örtüşmeyebilir, ama tüm 7
    /// modülde tutarlı, DB constraint gerektirmeyen bir kural — bkz. kapanış raporu "açık kararlar").</summary>
    public async Task<IReadOnlyList<SeoDuplicateGroupDto>> GetDuplicateSeoUrlReportAsync()
    {
        var result = new List<SeoDuplicateGroupDto>();

        foreach (var type in SupportedTypes)
        {
            var records = await GetRecordsAsync(type.EntityType);
            var byLanguageAndUrl = new Dictionary<(int LanguageId, string NormalizedUrl), (string LanguageCode, List<SeoRecordSummaryDto> Records)>();

            foreach (var record in records)
            {
                var detail = await GetRecordDetailAsync(type.EntityType, record.EntityId);
                if (detail is null)
                {
                    continue;
                }

                foreach (var language in detail.Languages)
                {
                    var normalized = SeoUrlNormalizer.Normalize(language.SeoUrl);
                    if (normalized.Length == 0)
                    {
                        continue;
                    }

                    var key = (language.LanguageId, normalized);
                    if (!byLanguageAndUrl.TryGetValue(key, out var entry))
                    {
                        entry = (language.LanguageCode, []);
                        byLanguageAndUrl[key] = entry;
                    }

                    entry.Records.Add(record);
                }
            }

            foreach (var ((_, normalizedUrl), (languageCode, matchingRecords)) in byLanguageAndUrl)
            {
                if (matchingRecords.Count > 1)
                {
                    result.Add(new SeoDuplicateGroupDto
                    {
                        EntityType = type.EntityType,
                        LanguageCode = languageCode,
                        NormalizedSeoUrl = normalizedUrl,
                        Records = matchingRecords
                    });
                }
            }
        }

        return result;
    }

    private async Task SetOrDeleteAsync(EntityType entityType, int entityId, int languageId, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            await _translationService.DeleteTranslationFieldAsync(entityType, entityId, languageId, fieldName);
        }
        else
        {
            await _translationService.SetTranslationAsync(entityType, entityId, languageId, fieldName, value.Trim());
        }
    }

    private static string SeoUrlFieldName(EntityType entityType) => entityType switch
    {
        EntityType.Product => ProductFields.SeoUrl,
        EntityType.Category => CategoryFields.SeoUrl,
        EntityType.Collection => CollectionFields.SeoUrl,
        EntityType.Blog => BlogFields.SeoUrl,
        EntityType.News => NewsFields.SeoUrl,
        EntityType.Page => PageFields.SeoUrl,
        EntityType.ReferenceProject => ReferenceProjectFields.SeoUrl,
        _ => throw new InvalidOperationException($"'{entityType}' SEO Yönetimi tarafından desteklenmiyor.")
    };

    private static string? MetaTitleFieldName(EntityType entityType) => entityType switch
    {
        EntityType.Product => ProductFields.MetaTitle,
        EntityType.Category => CategoryFields.MetaTitle,
        EntityType.Collection => CollectionFields.MetaTitle,
        EntityType.Blog => BlogFields.MetaTitle,
        EntityType.News => NewsFields.MetaTitle,
        EntityType.Page => PageFields.MetaTitle,
        _ => null
    };

    private static string? MetaDescriptionFieldName(EntityType entityType) => entityType switch
    {
        EntityType.Product => ProductFields.MetaDescription,
        EntityType.Category => CategoryFields.MetaDescription,
        EntityType.Collection => CollectionFields.MetaDescription,
        EntityType.Blog => BlogFields.MetaDescription,
        EntityType.News => NewsFields.MetaDescription,
        EntityType.Page => PageFields.MetaDescription,
        _ => null
    };
}
