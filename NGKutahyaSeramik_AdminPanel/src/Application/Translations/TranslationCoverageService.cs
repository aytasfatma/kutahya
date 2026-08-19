using Application.Banners;
using Application.Blogs;
using Application.Categories;
using Application.Collections;
using Application.News;
using Application.Pages;
using Application.Products;
using Application.ReferenceProjects;
using Domain.Enums;

namespace Application.Translations;

/// <summary>
/// ADR-007'nin ertelenen kısmının uygulanışı: "eksik çeviri raporları ve dashboard uyarıları" merkezi
/// tespit katmanı. Kapsam SeoManagementService'in 7 tipinden GENİŞ — SEO alanlarıyla sınırlı değil,
/// her modülün TÜM Translation alanları (*Fields.cs sabitleri, Task 21/22 sonrası kod taramasıyla
/// doğrulandı) taranır. Dealer kasıtlı olarak dışarıda — DealerFields.cs hiç yok (ADR-008: Dealer'ın
/// hiçbir alanı Translation'a taşınmadı). Her modül servisinin GetAllAsync/GetTreeAsync'i zaten TÜM
/// aktif diller için bir Translations satırı döndürüyor (alan boşsa Value null) — bu yüzden ayrı bir
/// ITranslationService.GetTranslationsAsync çağrısına gerek yok, doğrudan bu DTO'lar üzerinden okunur.
/// SADECE TESPİT — fallback/başka dilden doldurma YOK (görev talimatı, ADR-007 ile tutarlı).
/// </summary>
public class TranslationCoverageService
{
    /// <summary>Her EntityType kendi ayrı, özgül etiketiyle — SeoManagementService.SupportedTypes'ın
    /// deseniyle aynı (Kategori/Koleksiyon RBAC'ta tek "Koleksiyon/Kategori Yönetimi" modülü olsa da,
    /// burada "Modüle göre eksik sayısı" içerik TÜRÜ bazında anlamlı; iki ayrı EntityType tek etiket
    /// altında toplanırsa hangi türün gerçekten eksik olduğu belirsizleşirdi).</summary>
    private static readonly IReadOnlyList<(EntityType Type, string Label)> SupportedTypes =
    [
        (EntityType.Product, "Ürün"),
        (EntityType.Blog, "Blog"),
        (EntityType.BlogCategory, "Blog Kategorisi"),
        (EntityType.News, "Bülten"),
        (EntityType.NewsCategory, "Bülten Kategorisi"),
        (EntityType.Page, "Sayfa"),
        (EntityType.PageContentBlock, "Sayfa İçerik Bloğu"),
        (EntityType.Banner, "Banner"),
        (EntityType.ReferenceProject, "Referans Proje")
    ];

    private readonly ProductService _productService;
    private readonly CategoryService _categoryService;
    private readonly CollectionService _collectionService;
    private readonly BlogService _blogService;
    private readonly BlogCategoryService _blogCategoryService;
    private readonly NewsService _newsService;
    private readonly NewsCategoryService _newsCategoryService;
    private readonly PageService _pageService;
    private readonly PageContentBlockService _pageContentBlockService;
    private readonly BannerService _bannerService;
    private readonly ReferenceProjectService _referenceProjectService;
    private readonly ITranslationService _translationService;
    private int _totalRequiredFields;

    public TranslationCoverageService(
        ProductService productService,
        CategoryService categoryService,
        CollectionService collectionService,
        BlogService blogService,
        BlogCategoryService blogCategoryService,
        NewsService newsService,
        NewsCategoryService newsCategoryService,
        PageService pageService,
        PageContentBlockService pageContentBlockService,
        BannerService bannerService,
        ReferenceProjectService referenceProjectService,
        ITranslationService translationService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _collectionService = collectionService;
        _blogService = blogService;
        _blogCategoryService = blogCategoryService;
        _newsService = newsService;
        _newsCategoryService = newsCategoryService;
        _pageService = pageService;
        _pageContentBlockService = pageContentBlockService;
        _bannerService = bannerService;
        _referenceProjectService = referenceProjectService;
        _translationService = translationService;
    }

    public IReadOnlyList<(EntityType Type, string Label)> GetSupportedTypes() => SupportedTypes;

    public async Task<TranslationCoverageReportDto> GetReportAsync()
    {
        var activeLanguages = await _translationService.GetActiveLanguagesAsync();
        var items = new List<MissingTranslationDto>();
        _totalRequiredFields = 0;

        items.AddRange(await CollectProductAsync());
        items.AddRange(await CollectBlogAsync());
        items.AddRange(await CollectBlogCategoryAsync());
        items.AddRange(await CollectNewsAsync());
        items.AddRange(await CollectNewsCategoryAsync());
        items.AddRange(await CollectPageAsync());
        items.AddRange(await CollectPageContentBlockAsync());
        items.AddRange(await CollectBannerAsync());
        items.AddRange(await CollectReferenceProjectAsync());

        return TranslationCoverageReportBuilder.Build(items, activeLanguages, SupportedTypes, _totalRequiredFields);
    }

    private async Task<List<MissingTranslationDto>> CollectProductAsync()
    {
        const string label = "Ürün";
        var result = new List<MissingTranslationDto>();

        foreach (var product in await _productService.GetAllAsync())
        {
            var displayName = product.DisplayName ?? product.ProductCode;
            foreach (var t in product.Translations)
            {
                AddIfMissing(result, EntityType.Product, label, product.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, ProductFields.Name, t.Name);
                AddIfMissing(result, EntityType.Product, label, product.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, ProductFields.ShortDescription, t.ShortDescription);
                AddIfMissing(result, EntityType.Product, label, product.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, ProductFields.LongDescription, t.LongDescription);
                AddIfMissing(result, EntityType.Product, label, product.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, ProductFields.SeoUrl, t.SeoUrl);
                AddIfMissing(result, EntityType.Product, label, product.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, ProductFields.MetaTitle, t.MetaTitle);
                AddIfMissing(result, EntityType.Product, label, product.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, ProductFields.MetaDescription, t.MetaDescription);
            }
        }

        return result;
    }

    private async Task<List<MissingTranslationDto>> CollectCategoryAsync()
    {
        const string label = "Kategori";
        var result = new List<MissingTranslationDto>();

        foreach (var category in await _categoryService.GetTreeAsync())
        {
            var displayName = category.DisplayName ?? $"#{category.Id}";
            foreach (var t in category.Translations)
            {
                AddIfMissing(result, EntityType.Category, label, category.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, CategoryFields.Name, t.Name);
                AddIfMissing(result, EntityType.Category, label, category.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, CategoryFields.Description, t.Description);
                AddIfMissing(result, EntityType.Category, label, category.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, CategoryFields.SeoUrl, t.SeoUrl);
                AddIfMissing(result, EntityType.Category, label, category.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, CategoryFields.MetaTitle, t.MetaTitle);
                AddIfMissing(result, EntityType.Category, label, category.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, CategoryFields.MetaDescription, t.MetaDescription);
            }
        }

        return result;
    }

    private async Task<List<MissingTranslationDto>> CollectCollectionAsync()
    {
        const string label = "Koleksiyon";
        var result = new List<MissingTranslationDto>();

        foreach (var collection in await _collectionService.GetAllAsync())
        {
            var displayName = collection.DisplayName ?? $"#{collection.Id}";
            foreach (var t in collection.Translations)
            {
                AddIfMissing(result, EntityType.Collection, label, collection.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, CollectionFields.Name, t.Name);
                AddIfMissing(result, EntityType.Collection, label, collection.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, CollectionFields.Description, t.Description);
                AddIfMissing(result, EntityType.Collection, label, collection.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, CollectionFields.SeoUrl, t.SeoUrl);
                AddIfMissing(result, EntityType.Collection, label, collection.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, CollectionFields.MetaTitle, t.MetaTitle);
                AddIfMissing(result, EntityType.Collection, label, collection.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, CollectionFields.MetaDescription, t.MetaDescription);
            }
        }

        return result;
    }

    private async Task<List<MissingTranslationDto>> CollectBlogAsync()
    {
        const string label = "Blog";
        var result = new List<MissingTranslationDto>();

        foreach (var blog in await _blogService.GetAllAsync())
        {
            var displayName = blog.DisplayTitle ?? $"#{blog.Id}";
            foreach (var t in blog.Translations)
            {
                AddIfMissing(result, EntityType.Blog, label, blog.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, BlogFields.Title, t.Title);
                AddIfMissing(result, EntityType.Blog, label, blog.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, BlogFields.Excerpt, t.Excerpt);
                AddIfMissing(result, EntityType.Blog, label, blog.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, BlogFields.Content, t.Content);
                AddIfMissing(result, EntityType.Blog, label, blog.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, BlogFields.SeoUrl, t.SeoUrl);
                AddIfMissing(result, EntityType.Blog, label, blog.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, BlogFields.MetaTitle, t.MetaTitle);
                AddIfMissing(result, EntityType.Blog, label, blog.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, BlogFields.MetaDescription, t.MetaDescription);
            }
        }

        return result;
    }

    private async Task<List<MissingTranslationDto>> CollectBlogCategoryAsync()
    {
        const string label = "Blog Kategorisi";
        var result = new List<MissingTranslationDto>();

        foreach (var category in await _blogCategoryService.GetAllAsync())
        {
            var displayName = category.DisplayName ?? $"#{category.Id}";
            foreach (var t in category.Translations)
            {
                AddIfMissing(result, EntityType.BlogCategory, label, category.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, BlogCategoryFields.Name, t.Name);
            }
        }

        return result;
    }

    private async Task<List<MissingTranslationDto>> CollectNewsAsync()
    {
        const string label = "Bülten";
        var result = new List<MissingTranslationDto>();

        foreach (var news in await _newsService.GetAllAsync())
        {
            var displayName = news.DisplayTitle ?? $"#{news.Id}";
            foreach (var t in news.Translations)
            {
                AddIfMissing(result, EntityType.News, label, news.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, NewsFields.Title, t.Title);
                AddIfMissing(result, EntityType.News, label, news.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, NewsFields.Content, t.Content);
                AddIfMissing(result, EntityType.News, label, news.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, NewsFields.SeoUrl, t.SeoUrl);
                AddIfMissing(result, EntityType.News, label, news.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, NewsFields.MetaTitle, t.MetaTitle);
                AddIfMissing(result, EntityType.News, label, news.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, NewsFields.MetaDescription, t.MetaDescription);
            }
        }

        return result;
    }

    private async Task<List<MissingTranslationDto>> CollectNewsCategoryAsync()
    {
        const string label = "Bülten Kategorisi";
        var result = new List<MissingTranslationDto>();

        foreach (var category in await _newsCategoryService.GetAllAsync())
        {
            var displayName = category.DisplayName ?? $"#{category.Id}";
            foreach (var t in category.Translations)
            {
                AddIfMissing(result, EntityType.NewsCategory, label, category.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, NewsCategoryFields.Name, t.Name);
            }
        }

        return result;
    }

    private async Task<List<MissingTranslationDto>> CollectPageAsync()
    {
        const string label = "Sayfa";
        var result = new List<MissingTranslationDto>();

        foreach (var page in await _pageService.GetAllAsync())
        {
            var displayName = page.DisplayTitle ?? $"#{page.Id}";
            foreach (var t in page.Translations)
            {
                AddIfMissing(result, EntityType.Page, label, page.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, PageFields.Title, t.Title);
                AddIfMissing(result, EntityType.Page, label, page.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, PageFields.SeoUrl, t.SeoUrl);
                AddIfMissing(result, EntityType.Page, label, page.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, PageFields.MetaTitle, t.MetaTitle);
                AddIfMissing(result, EntityType.Page, label, page.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, PageFields.MetaDescription, t.MetaDescription);
            }
        }

        return result;
    }

    /// <summary>PageContentBlockService'in tek liste metodu sayfa-kapsamlı (GetByPageIdAsync) —
    /// global bir GetAllAsync yok, bu yüzden önce tüm sayfalar, sonra her sayfanın blokları gezilir.</summary>
    private async Task<List<MissingTranslationDto>> CollectPageContentBlockAsync()
    {
        const string label = "Sayfa İçerik Bloğu";
        var result = new List<MissingTranslationDto>();

        foreach (var page in await _pageService.GetAllAsync())
        {
            var pageTitle = page.DisplayTitle ?? $"#{page.Id}";
            foreach (var block in await _pageContentBlockService.GetByPageIdAsync(page.Id))
            {
                var displayName = $"{pageTitle} — Blok #{block.DisplayOrder + 1}";
                foreach (var t in block.Translations)
                {
                    AddIfMissing(result, EntityType.PageContentBlock, label, block.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, PageContentBlockFields.Title, t.Title, page.Id);
                    AddIfMissing(result, EntityType.PageContentBlock, label, block.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, PageContentBlockFields.Content, t.Content, page.Id);
                }
            }
        }

        return result;
    }

    private async Task<List<MissingTranslationDto>> CollectBannerAsync()
    {
        const string label = "Banner";
        var result = new List<MissingTranslationDto>();

        foreach (var banner in await _bannerService.GetAllAsync())
        {
            var displayName = banner.DisplayName ?? $"#{banner.Id}";
            foreach (var t in banner.Translations)
            {
                AddIfMissing(result, EntityType.Banner, label, banner.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, BannerFields.Title, t.Title);
                AddIfMissing(result, EntityType.Banner, label, banner.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, BannerFields.Subtitle, t.Subtitle);
                AddIfMissing(result, EntityType.Banner, label, banner.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, BannerFields.ButtonText, t.ButtonText);
                AddIfMissing(result, EntityType.Banner, label, banner.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, BannerFields.ButtonUrl, t.ButtonUrl);
            }
        }

        return result;
    }

    private async Task<List<MissingTranslationDto>> CollectReferenceProjectAsync()
    {
        const string label = "Referans Proje";
        var result = new List<MissingTranslationDto>();

        foreach (var referenceProject in await _referenceProjectService.GetAllAsync())
        {
            var displayName = referenceProject.DisplayName ?? $"#{referenceProject.Id}";
            foreach (var t in referenceProject.Translations)
            {
                AddIfMissing(result, EntityType.ReferenceProject, label, referenceProject.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, ReferenceProjectFields.ProjectName, t.ProjectName);
                AddIfMissing(result, EntityType.ReferenceProject, label, referenceProject.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, ReferenceProjectFields.Description, t.Description);
                AddIfMissing(result, EntityType.ReferenceProject, label, referenceProject.Id, displayName, t.LanguageId, t.LanguageCode, t.LanguageName, ReferenceProjectFields.SeoUrl, t.SeoUrl);
            }
        }

        return result;
    }

    private void AddIfMissing(
        List<MissingTranslationDto> result,
        EntityType entityType,
        string moduleLabel,
        int entityId,
        string displayName,
        int languageId,
        string languageCode,
        string languageName,
        string fieldName,
        string? value,
        int? parentEntityId = null)
    {
        _totalRequiredFields++;

        if (string.IsNullOrWhiteSpace(value))
        {
            result.Add(new MissingTranslationDto
            {
                EntityType = entityType,
                ModuleLabel = moduleLabel,
                EntityId = entityId,
                DisplayName = displayName,
                LanguageId = languageId,
                LanguageCode = languageCode,
                LanguageName = languageName,
                FieldName = fieldName,
                ParentEntityId = parentEntityId
            });
        }
    }
}
