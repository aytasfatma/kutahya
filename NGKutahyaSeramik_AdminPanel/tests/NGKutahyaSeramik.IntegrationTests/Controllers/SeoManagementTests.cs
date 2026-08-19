using Application.Categories;
using Application.Collections;
using Application.Products;
using Application.ReferenceProjects;
using Application.Seo;
using Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NGKutahyaSeramik.IntegrationTests.Authentication;
using NGKutahyaSeramik.IntegrationTests.Fixtures;
using NGKutahyaSeramik.IntegrationTests.Security;

namespace NGKutahyaSeramik.IntegrationTests.Controllers;

/// <summary>
/// Backlog #4 — SEO veri sözleşmesi + SEO Yönetimi. Gerçek HTTP pipeline'ı üzerinden RBAC/AntiForgery/
/// PRG doğrulanır; ayrıca dil-bazlı güncelleme, geçersiz entity/entityId reddi, ve tipin desteklemediği
/// alanların (ReferenceProject → MetaTitle/MetaDescription) sessizce yok sayıldığı sınanır.
/// Her test kendi factory'sini kullanır (SEO verisini değiştiren testler arası izolasyon için).
/// </summary>
public class SeoManagementTests
{
    private static async Task<(int ProductId, int TrLanguageId)> CreateTestProductAsync(CustomWebApplicationFactory factory, string productCode)
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
            Translations = [new CategoryTranslationInput { LanguageId = tr.Id, Name = "SEO Test Kategori " + productCode }]
        });
        var category = (await categoryService.GetTreeAsync()).Single(c => c.DisplayName == "SEO Test Kategori " + productCode);

        await collectionService.CreateAsync(new CreateCollectionRequest
        {
            DisplayOrder = await collectionService.GetNextDisplayOrderAsync(),
            Translations = [new CollectionTranslationInput { LanguageId = tr.Id, Name = "SEO Test Koleksiyon " + productCode }]
        });
        var collection = (await collectionService.GetAllAsync()).Single(c => c.DisplayName == "SEO Test Koleksiyon " + productCode);

        await productService.CreateAsync(new CreateProductRequest
        {
            ProductCode = productCode,
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
            Translations = [new ProductTranslationInput { LanguageId = tr.Id, Name = productCode, ShortDescription = "Kısa açıklama" }]
        });

        var product = (await productService.GetAllAsync()).Single(p => p.ProductCode == productCode);
        return (product.Id, tr.Id);
    }

    private static async Task<int> CreateTestReferenceProjectAsync(CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var referenceProjectService = scope.ServiceProvider.GetRequiredService<ReferenceProjectService>();

        var languages = await referenceProjectService.GetActiveLanguagesAsync();
        var tr = languages.Single(l => l.Code == "TR");

        await referenceProjectService.CreateAsync(new CreateReferenceProjectRequest
        {
            ProjectType = ReferenceProjectType.Konut,
            DisplayOrder = await referenceProjectService.GetNextDisplayOrderAsync(),
            Translations = [new ReferenceProjectTranslationInput { LanguageId = tr.Id, ProjectName = "SEO Test Referans Proje" }]
        });

        var project = (await referenceProjectService.GetAllAsync()).Single();
        return project.Id;
    }

    [Fact]
    public async Task Index_AllowedFor_Admin()
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin().GetAsync("/Seo");

        ((int)response.StatusCode).Should().Be(200);
    }

    [Fact]
    public async Task Index_AllowedFor_SeoEditor()
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsSeoEditor().GetAsync("/Seo");

        ((int)response.StatusCode).Should().Be(200);
    }

    // SEO Yönetimi merkezi ekranı yalnızca Admin+SEO Editörü'ne açık — İçerik Editörü/Ürün Yöneticisi
    // kendi modüllerindeki SEO alanlarına Task 20'nin alan-seviyeli RBAC'ı üzerinden zaten erişiyor.
    [Theory]
    [InlineData("İçerik Editörü")]
    [InlineData("Ürün Yöneticisi")]
    public async Task Index_DeniedFor_ContentEditorAndProductManager(string role)
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsRole(role).GetAsync("/Seo");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task Index_AnonymousAccess_IsDenied()
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsAnonymous().GetAsync("/Seo");

        ((int)response.StatusCode).Should().BeOneOf(401, 302, 403);
    }

    [Fact]
    public async Task Edit_Product_UpdatesSeoUrlAndMetaFields_ForSpecificLanguage()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (productId, trLanguageId) = await CreateTestProductAsync(factory, "SEO-TEST-0001");

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsSeoEditor();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: $"/Seo/Edit/{productId}?type=Product",
            postUrl: $"/Seo/Edit/{productId}?type=Product",
            formValues: new Dictionary<string, string>
            {
                ["Languages[0].LanguageId"] = trLanguageId.ToString(),
                ["Languages[0].SeoUrl"] = "test-urun-seo-url",
                ["Languages[0].MetaTitle"] = "Test Meta Başlık",
                ["Languages[0].MetaDescription"] = "Test meta açıklama"
            });

        ((int)response.StatusCode).Should().Be(302, "başarılı güncelleme → PRG");

        using var scope = factory.Services.CreateScope();
        var seoService = scope.ServiceProvider.GetRequiredService<SeoManagementService>();
        var detail = await seoService.GetRecordDetailAsync(EntityType.Product, productId);
        var tr = detail!.Languages.Single(l => l.LanguageId == trLanguageId);
        tr.SeoUrl.Should().Be("test-urun-seo-url");
        tr.MetaTitle.Should().Be("Test Meta Başlık");
        tr.MetaDescription.Should().Be("Test meta açıklama");
    }

    // ReferenceProjectFields.cs'te MetaTitle/MetaDescription hiç yok — servis bunları sessizce yok
    // saymalı (hata değil), yalnızca SeoUrl kaydedilmeli.
    [Fact]
    public async Task Edit_ReferenceProject_OnlySavesSeoUrl_IgnoresMetaFields()
    {
        await using var factory = new CustomWebApplicationFactory();
        var projectId = await CreateTestReferenceProjectAsync(factory);

        using (var scope = factory.Services.CreateScope())
        {
            var seoService = scope.ServiceProvider.GetRequiredService<SeoManagementService>();
            var detail = await seoService.GetRecordDetailAsync(EntityType.ReferenceProject, projectId);
            detail!.SupportsMetaFields.Should().BeFalse();

            var trLangId = detail.Languages.First(l => l.LanguageCode == "TR").LanguageId;

            var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsSeoEditor();
            var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
                client,
                formUrl: $"/Seo/Edit/{projectId}?type=ReferenceProject",
                postUrl: $"/Seo/Edit/{projectId}?type=ReferenceProject",
                formValues: new Dictionary<string, string>
                {
                    ["Languages[0].LanguageId"] = trLangId.ToString(),
                    ["Languages[0].SeoUrl"] = "test-referans-proje-seo-url",
                    ["Languages[0].MetaTitle"] = "Bu Kaydedilmemeli",
                    ["Languages[0].MetaDescription"] = "Bu da Kaydedilmemeli"
                });

            ((int)response.StatusCode).Should().Be(302);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var seoService = scope.ServiceProvider.GetRequiredService<SeoManagementService>();
            var updated = await seoService.GetRecordDetailAsync(EntityType.ReferenceProject, projectId);
            var tr = updated!.Languages.Single(l => l.LanguageCode == "TR");
            tr.SeoUrl.Should().Be("test-referans-proje-seo-url");
            tr.MetaTitle.Should().BeNull("ReferenceProject MetaTitle desteklemiyor");
            tr.MetaDescription.Should().BeNull("ReferenceProject MetaDescription desteklemiyor");
        }
    }

    [Fact]
    public async Task Edit_UnknownEntityId_ReturnsErrorAndRedirects()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();

        var response = await client.GetAsync("/Seo/Edit/999999?type=Product");

        ((int)response.StatusCode).Should().Be(302, "bulunamayan kayıt Index'e yönlendirilmeli");
    }

    [Fact]
    public async Task DuplicateSeoUrlReport_DetectsSameNormalizedUrl_SameTypeSameLanguage()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (product1Id, trLanguageId) = await CreateTestProductAsync(factory, "SEO-DUP-0001");
        var (product2Id, _) = await CreateTestProductAsync(factory, "SEO-DUP-0002");

        using (var scope = factory.Services.CreateScope())
        {
            var seoService = scope.ServiceProvider.GetRequiredService<SeoManagementService>();
            await seoService.UpdateAsync(EntityType.Product, product1Id, trLanguageId, "Aynı SEO URL", null, null);
            await seoService.UpdateAsync(EntityType.Product, product2Id, trLanguageId, "aynı-seo-url", null, null);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var seoService = scope.ServiceProvider.GetRequiredService<SeoManagementService>();
            var duplicates = await seoService.GetDuplicateSeoUrlReportAsync();

            var group = duplicates.Should().ContainSingle(g =>
                g.EntityType == EntityType.Product && g.LanguageCode == "TR" && g.NormalizedSeoUrl == "ayni-seo-url")
                .Subject;
            group.Records.Should().HaveCount(2);
            group.Records.Select(r => r.EntityId).Should().BeEquivalentTo([product1Id, product2Id]);
        }
    }

    [Fact]
    public async Task MissingSeoReport_ListsRecordWithBlankSeoUrl()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (productId, _) = await CreateTestProductAsync(factory, "SEO-MISSING-0001");

        using var scope = factory.Services.CreateScope();
        var seoService = scope.ServiceProvider.GetRequiredService<SeoManagementService>();
        var missing = await seoService.GetMissingSeoReportAsync();

        missing.Should().Contain(m => m.EntityType == EntityType.Product && m.EntityId == productId && m.LanguageCode == "TR" &&
            m.MissingFields.Contains("SeoUrl") && m.MissingFields.Contains("MetaTitle") && m.MissingFields.Contains("MetaDescription"));
    }

    [Fact]
    public async Task Edit_WithoutAntiForgeryToken_IsRejected()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (productId, trLanguageId) = await CreateTestProductAsync(factory, "SEO-NOAF-0001");

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();
        var response = await client.PostAsync($"/Seo/Edit/{productId}?type=Product", new FormUrlEncodedContent(
            new Dictionary<string, string> { ["Languages[0].LanguageId"] = trLanguageId.ToString() }));

        ((int)response.StatusCode).Should().Be(400);
    }

    [Fact]
    public async Task MissingReport_And_Duplicates_AllowedFor_SeoEditor()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsSeoEditor();

        ((int)(await client.GetAsync("/Seo/MissingReport")).StatusCode).Should().Be(200);
        ((int)(await client.GetAsync("/Seo/Duplicates")).StatusCode).Should().Be(200);
    }

    // EntityType doğrulaması — Index zaten IsSupported kontrolü yapıyordu, Edit(GET) yapmıyordu:
    // desteklenmeyen bir tip (Banner'ın hiç SEO alanı yok) doğrudan URL ile istenirse eskiden
    // GetRecordsAsync'in InvalidOperationException'ı ile 500 patlıyordu — artık Index'teki gibi
    // dostane bir redirect'e düşmeli.
    [Fact]
    public async Task Edit_UnsupportedEntityType_RedirectsInsteadOfThrowing()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();

        var response = await client.GetAsync("/Seo/Edit/1?type=Banner");

        ((int)response.StatusCode).Should().Be(302, "Banner'ın hiç SEO alanı yok (BannerFields.cs) — 500 yerine Index'e yönlendirilmeli");
        response.Headers.Location!.ToString().Should().Contain("/Seo");
    }

    // Dil doğrulaması — languageId aktif dil listesinde olmalı; olmayan bir Id ile POST edilirse
    // (ör. hiç var olmayan 999999) eskiden Translation FK ihlaliyle SaveChangesAsync'te 500 patlıyordu,
    // artık UpdateAsync bunu kullanıcıya anlaşılır bir hatayla (form 200 ile yeniden render) reddetmeli.
    [Fact]
    public async Task Edit_InvalidLanguageId_IsRejected_ViaService()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (productId, _) = await CreateTestProductAsync(factory, "SEO-BADLANG-01");

        using var scope = factory.Services.CreateScope();
        var seoService = scope.ServiceProvider.GetRequiredService<SeoManagementService>();

        var (succeeded, errorMessage, _) = await seoService.UpdateAsync(EntityType.Product, productId, 999999, "gecersiz-dil-url", null, null);

        succeeded.Should().BeFalse();
        errorMessage.Should().NotBeNullOrEmpty();

        var detail = await seoService.GetRecordDetailAsync(EntityType.Product, productId);
        detail!.Languages.Should().NotContain(l => l.SeoUrl == "gecersiz-dil-url", "geçersiz dille yapılan yazma denemesi hiçbir kayda yansımamalı");
    }

    [Fact]
    public async Task Edit_InvalidLanguageId_IsRejected_ViaHttp_FormReRendersWithError()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (productId, _) = await CreateTestProductAsync(factory, "SEO-BADLANG-02");

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: $"/Seo/Edit/{productId}?type=Product",
            postUrl: $"/Seo/Edit/{productId}?type=Product",
            formValues: new Dictionary<string, string>
            {
                ["Languages[0].LanguageId"] = "999999",
                ["Languages[0].SeoUrl"] = "http-gecersiz-dil-url"
            });

        ((int)response.StatusCode).Should().Be(200, "reddedilen güncelleme PRG uygulamaz, form hata mesajıyla tekrar render edilir (500 değil)");
    }
}
