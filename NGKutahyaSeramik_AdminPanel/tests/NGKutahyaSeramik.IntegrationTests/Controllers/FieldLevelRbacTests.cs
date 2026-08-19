using Application.Categories;
using Application.Collections;
using Application.Pages;
using Application.Products;
using Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NGKutahyaSeramik.IntegrationTests.Authentication;
using NGKutahyaSeramik.IntegrationTests.Fixtures;
using NGKutahyaSeramik.IntegrationTests.Security;

namespace NGKutahyaSeramik.IntegrationTests.Controllers;

/// <summary>
/// Backlog #23 — Alan-seviyeli RBAC. Ürün Yönetimi'nde İçerik Editörü (Name/ShortDescription/
/// LongDescription) ve SEO Editörü (SeoUrl/MetaTitle/MetaDescription), Sayfa Yönetimi'nde SEO
/// Editörü (SeoUrl/MetaTitle/MetaDescription) yalnızca izinli alanları değiştirebiliyor mu; izinsiz
/// (native + diğer translation) alanlar POST edilse bile DEĞİŞMİYOR mu — gerçek HTTP pipeline'ı
/// (AntiForgery hiç devre dışı bırakılmadan, PRG dahil) üzerinden doğrular. Admin/Ürün Yöneticisi
/// mevcut tam yetkisini koruduğunu da ayrıca sınar (regresyon).
///
/// Her test kendi `CustomWebApplicationFactory`'sini kullanır (UserManagementTests.cs'teki desenle
/// aynı gerekçe: testler arası izolasyon, paylaşılan state yok).
/// </summary>
public class FieldLevelRbacTests
{
    private static Dictionary<string, string> FullProductFormValues(
        int productId, int trLanguageId, string productCode, string size, string displayOrder, string name) => new()
    {
        ["ProductCode"] = productCode,
        ["CategoryId"] = "1",
        ["CollectionId"] = "1",
        ["Brand"] = "NgSeramik",
        ["Brands[0]"] = "NgSeramik",
        ["Status"] = "Active",
        ["Size"] = size,
        ["Unit"] = "m2",
        ["Surface"] = "Mat",
        ["BodyType"] = "Porselen",
        ["Color"] = "Beyaz",
        ["ApplicationArea"] = "Zemin",
        ["UsageArea"] = "İç Mekan",
        ["Thickness"] = "10",
        ["DisplayOrder"] = displayOrder,
        [$"Translations[0].LanguageId"] = trLanguageId.ToString(),
        [$"Translations[0].Name"] = name,
        [$"Translations[0].ShortDescription"] = "Kısa açıklama (Admin/PM testi)"
    };

    private static async Task<(int ProductId, int TrLanguageId)> CreateTestProductAsync(CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var categoryService = scope.ServiceProvider.GetRequiredService<CategoryService>();
        var collectionService = scope.ServiceProvider.GetRequiredService<CollectionService>();
        var productService = scope.ServiceProvider.GetRequiredService<ProductService>();

        var languages = await productService.GetActiveLanguagesAsync();
        var tr = languages.Single(l => l.Code == "TR");

        await categoryService.CreateAsync(new CreateCategoryRequest
        {
            DisplayOrder = await categoryService.GetNextDisplayOrderAsync(null),
            Translations = [new CategoryTranslationInput { LanguageId = tr.Id, Name = "FLR Test Kategori" }]
        });
        var category = (await categoryService.GetTreeAsync()).Single();

        await collectionService.CreateAsync(new CreateCollectionRequest
        {
            DisplayOrder = await collectionService.GetNextDisplayOrderAsync(),
            Translations = [new CollectionTranslationInput { LanguageId = tr.Id, Name = "FLR Test Koleksiyon" }]
        });
        var collection = (await collectionService.GetAllAsync()).Single();

        await productService.CreateAsync(new CreateProductRequest
        {
            ProductCode = "FLR-TEST-0001",
            CategoryId = category.Id,
            CollectionId = collection.Id,
            Brand = ProductBrand.NgSeramik,
            Status = ProductStatus.Active,
            Size = "60x60",
            Unit = "m2",
            Surface = "Mat",
            BodyType = "Porselen",
            Color = "Beyaz",
            ApplicationArea = "Zemin",
            UsageArea = "İç Mekan",
            Thickness = 10,
            DisplayOrder = await productService.GetNextDisplayOrderAsync(),
            Translations =
            [
                new ProductTranslationInput
                {
                    LanguageId = tr.Id,
                    Name = "Orijinal Ad",
                    ShortDescription = "Orijinal kısa açıklama",
                    LongDescription = "Orijinal uzun açıklama",
                    SeoUrl = "orijinal-seo-url",
                    MetaTitle = "Orijinal Meta Başlık",
                    MetaDescription = "Orijinal meta açıklama"
                }
            ]
        });

        var product = (await productService.GetAllAsync()).Single();
        return (product.Id, tr.Id);
    }

    private static async Task<(int PageId, int TrLanguageId)> CreateTestPageAsync(CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var pageService = scope.ServiceProvider.GetRequiredService<PageService>();

        var languages = await pageService.GetActiveLanguagesAsync();
        var tr = languages.Single(l => l.Code == "TR");

        await pageService.CreateAsync(new CreatePageRequest
        {
            Translations =
            [
                new PageTranslationInput
                {
                    LanguageId = tr.Id,
                    Title = "Orijinal Başlık",
                    SeoUrl = "orijinal-seo-url",
                    MetaTitle = "Orijinal Meta Başlık",
                    MetaDescription = "Orijinal meta açıklama"
                }
            ]
        });

        var page = (await pageService.GetAllAsync()).Single();
        return (page.Id, tr.Id);
    }

    private static async Task<ProductDto> GetProductAsync(CustomWebApplicationFactory factory, int id)
    {
        using var scope = factory.Services.CreateScope();
        var productService = scope.ServiceProvider.GetRequiredService<ProductService>();
        return (await productService.GetByIdAsync(id))!;
    }

    private static async Task<PageDto> GetPageAsync(CustomWebApplicationFactory factory, int id)
    {
        using var scope = factory.Services.CreateScope();
        var pageService = scope.ServiceProvider.GetRequiredService<PageService>();
        return (await pageService.GetByIdAsync(id))!;
    }

    // 1. İçerik Editörü: yalnızca Name/ShortDescription/LongDescription değişir, native alanlar
    // (ProductCode dahil) ve SEO alanları POST edilse bile aynı kalır.
    [Fact]
    public async Task Product_Edit_ContentEditor_ChangesContentFields_NotNativeOrSeoFields()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (productId, trLanguageId) = await CreateTestProductAsync(factory);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsContentEditor();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: $"/Product/Edit/{productId}",
            postUrl: $"/Product/Edit/{productId}",
            formValues: new Dictionary<string, string>
            {
                ["ProductCode"] = "HACKED-CODE",
                ["CategoryId"] = "999",
                ["CollectionId"] = "999",
                ["Size"] = "HACKED-SIZE",
                ["DisplayOrder"] = "77",
                ["Brands[0]"] = "NgSeramik",
                [$"Translations[0].LanguageId"] = trLanguageId.ToString(),
                [$"Translations[0].Name"] = "Yeni İçerik Editörü Adı",
                [$"Translations[0].ShortDescription"] = "Yeni kısa açıklama",
                [$"Translations[0].LongDescription"] = "Yeni uzun açıklama",
                [$"Translations[0].SeoUrl"] = "hacked-seo-url",
                [$"Translations[0].MetaTitle"] = "Hacked Meta Başlık",
                [$"Translations[0].MetaDescription"] = "Hacked meta açıklama"
            });

        ((int)response.StatusCode).Should().Be(302, "izinli alanlar değiştiği için başarılı POST → PRG");

        var product = await GetProductAsync(factory, productId);
        product.ProductCode.Should().Be("FLR-TEST-0001", "native alan İçerik Editörü tarafından değiştirilemez");
        product.Size.Should().Be("60x60", "native alan İçerik Editörü tarafından değiştirilemez");
        product.DisplayOrder.Should().Be(1, "native alan İçerik Editörü tarafından değiştirilemez");

        var tr = product.Translations.Single(t => t.LanguageId == trLanguageId);
        tr.Name.Should().Be("Yeni İçerik Editörü Adı", "izinli alan değişmeli");
        tr.ShortDescription.Should().Be("Yeni kısa açıklama", "izinli alan değişmeli");
        tr.LongDescription.Should().Be("Yeni uzun açıklama", "izinli alan değişmeli");
        tr.SeoUrl.Should().Be("orijinal-seo-url", "SEO alanı İçerik Editörü tarafından değiştirilemez");
        tr.MetaTitle.Should().Be("Orijinal Meta Başlık", "SEO alanı İçerik Editörü tarafından değiştirilemez");
        tr.MetaDescription.Should().Be("Orijinal meta açıklama", "SEO alanı İçerik Editörü tarafından değiştirilemez");
    }

    // 2. SEO Editörü: yalnızca SeoUrl/MetaTitle/MetaDescription değişir, native alanlar ve içerik
    // alanları (Name dahil) POST edilse bile aynı kalır.
    [Fact]
    public async Task Product_Edit_SeoEditor_ChangesSeoFields_NotNativeOrContentFields()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (productId, trLanguageId) = await CreateTestProductAsync(factory);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsSeoEditor();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: $"/Product/Edit/{productId}",
            postUrl: $"/Product/Edit/{productId}",
            formValues: new Dictionary<string, string>
            {
                ["ProductCode"] = "HACKED-CODE",
                ["Size"] = "HACKED-SIZE",
                ["DisplayOrder"] = "77",
                ["Brands[0]"] = "NgSeramik",
                [$"Translations[0].LanguageId"] = trLanguageId.ToString(),
                [$"Translations[0].Name"] = "Hacked Ad",
                [$"Translations[0].ShortDescription"] = "Hacked kısa açıklama",
                [$"Translations[0].SeoUrl"] = "yeni-seo-editoru-url",
                [$"Translations[0].MetaTitle"] = "Yeni SEO Meta Başlık",
                [$"Translations[0].MetaDescription"] = "Yeni SEO meta açıklama"
            });

        ((int)response.StatusCode).Should().Be(302);

        var product = await GetProductAsync(factory, productId);
        product.ProductCode.Should().Be("FLR-TEST-0001");
        product.Size.Should().Be("60x60");
        product.DisplayOrder.Should().Be(1, "native alan SEO Editörü tarafından değiştirilemez");

        var tr = product.Translations.Single(t => t.LanguageId == trLanguageId);
        tr.Name.Should().Be("Orijinal Ad", "içerik alanı SEO Editörü tarafından değiştirilemez");
        tr.ShortDescription.Should().Be("Orijinal kısa açıklama", "içerik alanı SEO Editörü tarafından değiştirilemez");
        tr.SeoUrl.Should().Be("yeni-seo-editoru-url", "izinli alan değişmeli");
        tr.MetaTitle.Should().Be("Yeni SEO Meta Başlık", "izinli alan değişmeli");
        tr.MetaDescription.Should().Be("Yeni SEO meta açıklama", "izinli alan değişmeli");
    }

    // 3. Regresyon — Admin hâlâ tüm alanları (native + tüm translation alanları) değiştirebiliyor.
    [Fact]
    public async Task Product_Edit_Admin_ChangesAllFields()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (productId, trLanguageId) = await CreateTestProductAsync(factory);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: $"/Product/Edit/{productId}",
            postUrl: $"/Product/Edit/{productId}",
            formValues: FullProductFormValues(productId, trLanguageId, productCode: "ADMIN-CODE", size: "ADMIN-SIZE", displayOrder: "5", name: "Admin Ad"));

        ((int)response.StatusCode).Should().Be(302);

        var product = await GetProductAsync(factory, productId);
        product.ProductCode.Should().Be("ADMIN-CODE", "Admin native alanları değiştirebilmeli");
        product.Size.Should().Be("ADMIN-SIZE");
        product.DisplayOrder.Should().Be(5);
        product.Translations.Single(t => t.LanguageId == trLanguageId).Name.Should().Be("Admin Ad");
    }

    // 4. Regresyon — Ürün Yöneticisi hâlâ tüm alanları değiştirebiliyor.
    [Fact]
    public async Task Product_Edit_ProductManager_ChangesAllFields()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (productId, trLanguageId) = await CreateTestProductAsync(factory);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsProductManager();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: $"/Product/Edit/{productId}",
            postUrl: $"/Product/Edit/{productId}",
            formValues: FullProductFormValues(productId, trLanguageId, productCode: "PM-CODE", size: "PM-SIZE", displayOrder: "5", name: "PM Ad"));

        ((int)response.StatusCode).Should().Be(302);

        var product = await GetProductAsync(factory, productId);
        product.ProductCode.Should().Be("PM-CODE", "Ürün Yöneticisi native alanları değiştirebilmeli");
    }

    // 5. Regresyon — İçerik Editörü hâlâ yeni ürün OLUŞTURAMAZ (yalnızca Edit açıldı, Create değil).
    [Fact]
    public async Task Product_Create_ContentEditor_StillDenied()
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient(new() { AllowAutoRedirect = false })
            .AsContentEditor()
            .GetAsync("/Product/Create");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    // 6. SEO Editörü: Sayfa'da yalnızca SeoUrl/MetaTitle/MetaDescription değişir, Title aynı kalır.
    [Fact]
    public async Task Page_Edit_SeoEditor_ChangesSeoFields_NotTitle()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (pageId, trLanguageId) = await CreateTestPageAsync(factory);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsSeoEditor();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: $"/Page/Edit/{pageId}",
            postUrl: $"/Page/Edit/{pageId}",
            formValues: new Dictionary<string, string>
            {
                [$"Translations[0].LanguageId"] = trLanguageId.ToString(),
                [$"Translations[0].Title"] = "Hacked Başlık",
                [$"Translations[0].SeoUrl"] = "yeni-sayfa-seo-url",
                [$"Translations[0].MetaTitle"] = "Yeni Sayfa Meta Başlık",
                [$"Translations[0].MetaDescription"] = "Yeni sayfa meta açıklama"
            });

        ((int)response.StatusCode).Should().Be(302, "SEO Editörü artık Sayfa Edit'e erişebiliyor (backlog #23)");

        var page = await GetPageAsync(factory, pageId);
        var tr = page.Translations.Single(t => t.LanguageId == trLanguageId);
        tr.Title.Should().Be("Orijinal Başlık", "Title alanı SEO Editörü tarafından değiştirilemez");
        tr.SeoUrl.Should().Be("yeni-sayfa-seo-url", "izinli alan değişmeli");
        tr.MetaTitle.Should().Be("Yeni Sayfa Meta Başlık", "izinli alan değişmeli");
        tr.MetaDescription.Should().Be("Yeni sayfa meta açıklama", "izinli alan değişmeli");
    }

    // 7. Regresyon — SEO Editörü hâlâ yeni sayfa OLUŞTURAMAZ.
    [Fact]
    public async Task Page_Create_SeoEditor_StillDenied()
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient(new() { AllowAutoRedirect = false })
            .AsSeoEditor()
            .GetAsync("/Page/Create");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    // 8. Regresyon — İçerik Editörü hâlâ Sayfa'da Title dahil tüm alanları değiştirebiliyor.
    [Fact]
    public async Task Page_Edit_ContentEditor_ChangesTitle()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (pageId, trLanguageId) = await CreateTestPageAsync(factory);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsContentEditor();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: $"/Page/Edit/{pageId}",
            postUrl: $"/Page/Edit/{pageId}",
            formValues: new Dictionary<string, string>
            {
                [$"Translations[0].LanguageId"] = trLanguageId.ToString(),
                [$"Translations[0].Title"] = "İçerik Editörü Başlığı"
            });

        ((int)response.StatusCode).Should().Be(302);

        var page = await GetPageAsync(factory, pageId);
        page.Translations.Single(t => t.LanguageId == trLanguageId).Title.Should().Be("İçerik Editörü Başlığı");
    }

    // 9. Anonim erişim reddi — Edit action'ları da (yeni FieldEditRoles ile) hâlâ authentication gerektiriyor.
    [Theory]
    [InlineData("/Product/Edit/1")]
    [InlineData("/Page/Edit/1")]
    public async Task Edit_AnonymousAccess_IsDenied(string url)
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient(new() { AllowAutoRedirect = false })
            .AsAnonymous()
            .GetAsync(url);

        ((int)response.StatusCode).Should().BeOneOf(401, 302, 403);
    }

    // 10. AntiForgery — token'sız POST reddedilir (yeni açılan Edit yolu için de geçerli).
    [Fact]
    public async Task Product_Edit_ContentEditor_WithoutAntiForgeryToken_IsRejected()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (productId, trLanguageId) = await CreateTestProductAsync(factory);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsContentEditor();
        var response = await client.PostAsync($"/Product/Edit/{productId}", new FormUrlEncodedContent(
            new Dictionary<string, string> { [$"Translations[0].LanguageId"] = trLanguageId.ToString() }));

        ((int)response.StatusCode).Should().Be(400);
    }
}
