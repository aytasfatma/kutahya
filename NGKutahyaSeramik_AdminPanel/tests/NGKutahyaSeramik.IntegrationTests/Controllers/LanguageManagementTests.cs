using Application.Categories;
using Application.Collections;
using Application.Languages;
using Application.Products;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using NGKutahyaSeramik.IntegrationTests.Authentication;
using NGKutahyaSeramik.IntegrationTests.Fixtures;
using NGKutahyaSeramik.IntegrationTests.Security;
using NGKutahyaSeramik.UnitTests.Factories;

namespace NGKutahyaSeramik.IntegrationTests.Controllers;

/// <summary>
/// Backlog #3 — Dil Yönetimi panel modülü. `Program.cs`'in kendi başlangıç seed'i (SeedLanguagesAsync)
/// her `CustomWebApplicationFactory` için otomatik çalıştığından 7 dil (TR/EN/DE/FR/ES/AR/RU) zaten
/// hazır — ayrıca seed etmeye gerek yok. Her test kendi factory'sini kullanır (Language satırlarını
/// değiştiren testler arası izolasyon için, UserManagementTests.cs'teki desenle aynı gerekçe).
/// </summary>
public class LanguageManagementTests
{
    private static async Task<(int TrId, int EnId)> GetSeededLanguageIdsAsync(CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var languageService = scope.ServiceProvider.GetRequiredService<LanguageService>();
        var languages = await languageService.GetAllAsync();

        return (languages.Single(l => l.Code == "TR").Id, languages.Single(l => l.Code == "EN").Id);
    }

    /// <summary>Yalnızca TR Name/ShortDescription verilmiş bir ürün — EN (ve TR'nin geri kalan
    /// alanları) doğal olarak eksik kalır, gerçek `ProductService.CreateAsync` akışı üzerinden
    /// (Translation tablosuna elle satır eklemek yerine — üretimdeki gerçek yolu sınar).</summary>
    private static async Task<string> SeedProductWithOnlyTrTranslationAsync(CustomWebApplicationFactory factory, string productCode)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var category = CategoryFactory.CreateRoot();
        var collection = CollectionFactory.CreateValid();
        db.Categories.Add(category);
        db.Collections.Add(collection);
        await db.SaveChangesAsync();

        var productService = scope.ServiceProvider.GetRequiredService<ProductService>();
        var result = await productService.CreateAsync(new CreateProductRequest
        {
            ProductCode = productCode,
            CategoryId = category.Id,
            CollectionId = collection.Id,
            Brand = Domain.Enums.ProductBrand.NgSeramik,
            Status = Domain.Enums.ProductStatus.Active,
            Size = "60x120",
            Unit = "M2",
            Surface = "MAT",
            Thickness = 9.0m,
            BodyType = "SIRLI PORSELEN",
            Color = "Beyaz",
            ApplicationArea = "YER",
            UsageArea = "BANYO",
            DisplayOrder = 1,
            Translations = [new ProductTranslationInput { LanguageId = 1, Name = "Amazonit", ShortDescription = "Kısa açıklama" }]
        });

        result.Succeeded.Should().BeTrue(result.ErrorMessage);
        return productCode;
    }

    [Fact]
    public async Task Index_AllowedFor_Admin()
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin().GetAsync("/Language");

        ((int)response.StatusCode).Should().Be(200);
    }

    // Madde 7.2/30 — Dil Yönetimi yalnızca Admin'e açık (Bayi/Showroom'dan sonra projedeki ikinci
    // salt-Admin modül); diğer 3 rolün hiçbir erişimi yok (salt-görüntüleme dahil).
    [Theory]
    [InlineData("İçerik Editörü")]
    [InlineData("SEO Editörü")]
    [InlineData("Ürün Yöneticisi")]
    public async Task Index_DeniedFor_AllNonAdminRoles(string role)
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsRole(role).GetAsync("/Language");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task Index_AnonymousAccess_IsDenied()
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsAnonymous().GetAsync("/Language");

        ((int)response.StatusCode).Should().BeOneOf(401, 302, 403);
    }

    [Fact]
    public async Task Edit_ValidUpdate_Succeeds_AndPersists()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (_, enId) = await GetSeededLanguageIdsAsync(factory);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: $"/Language/Edit/{enId}",
            postUrl: $"/Language/Edit/{enId}",
            formValues: new Dictionary<string, string>
            {
                ["Name"] = "İngilizce (Güncellendi)",
                ["DisplayOrder"] = "9",
                ["IsActive"] = "true"
            });

        ((int)response.StatusCode).Should().Be(302, "geçerli güncelleme → PRG");
        response.Headers.Location!.ToString().Should().Contain("/Language");

        using var scope = factory.Services.CreateScope();
        var languageService = scope.ServiceProvider.GetRequiredService<LanguageService>();
        var updated = await languageService.GetByIdAsync(enId);
        updated!.Name.Should().Be("İngilizce (Güncellendi)");
        updated.DisplayOrder.Should().Be(9);
        updated.Code.Should().Be("EN", "Code hiçbir zaman değişmemeli");
    }

    // Kesin kural (görev talimatı) — "Türkçe devre dışı bırakılamaz": gerçek HTTP POST ile denenir,
    // servis reddeder, form 200 ile hata mesajıyla tekrar render edilir (PRG uygulanmaz), DB değişmez.
    [Fact]
    public async Task Edit_DeactivateTurkish_IsRejected_ViaHttp()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (trId, _) = await GetSeededLanguageIdsAsync(factory);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: $"/Language/Edit/{trId}",
            postUrl: $"/Language/Edit/{trId}",
            formValues: new Dictionary<string, string>
            {
                ["Name"] = "Türkçe",
                ["DisplayOrder"] = "0",
                ["IsActive"] = "false"
            });

        ((int)response.StatusCode).Should().Be(200, "reddedilen güncelleme PRG uygulamaz, form hata mesajıyla tekrar render edilir");

        using var scope = factory.Services.CreateScope();
        var languageService = scope.ServiceProvider.GetRequiredService<LanguageService>();
        (await languageService.GetByIdAsync(trId))!.IsActive.Should().BeTrue("TR reddedilen istekle pasif yapılamamalı");
    }

    [Fact]
    public async Task Edit_BlankName_IsRejected_FormReRendersWithError()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (_, enId) = await GetSeededLanguageIdsAsync(factory);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: $"/Language/Edit/{enId}",
            postUrl: $"/Language/Edit/{enId}",
            formValues: new Dictionary<string, string>
            {
                ["Name"] = "",
                ["DisplayOrder"] = "2",
                ["IsActive"] = "true"
            });

        ((int)response.StatusCode).Should().Be(200);

        using var scope = factory.Services.CreateScope();
        var languageService = scope.ServiceProvider.GetRequiredService<LanguageService>();
        (await languageService.GetByIdAsync(enId))!.Name.Should().Be("English", "reddedilen istek DB'yi değiştirmemeli");
    }

    [Theory]
    [InlineData("İçerik Editörü")]
    [InlineData("SEO Editörü")]
    [InlineData("Ürün Yöneticisi")]
    public async Task Edit_DeniedFor_AllNonAdminRoles(string role)
    {
        await using var factory = new CustomWebApplicationFactory();
        var (_, enId) = await GetSeededLanguageIdsAsync(factory);

        var response = await factory.CreateClient(new() { AllowAutoRedirect = false })
            .AsRole(role)
            .GetAsync($"/Language/Edit/{enId}");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task Edit_WithoutAntiForgeryToken_IsRejected()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (_, enId) = await GetSeededLanguageIdsAsync(factory);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();
        var response = await client.PostAsync($"/Language/Edit/{enId}", new FormUrlEncodedContent(
            new Dictionary<string, string> { ["Name"] = "Token Yok", ["DisplayOrder"] = "2", ["IsActive"] = "true" }));

        ((int)response.StatusCode).Should().Be(400);
    }

    // Seed edilmiş diller silinemez — Controller'da hiçbir Delete action'ı yok, bu yüzden route
    // hiç var olmuyor (404), "yetkisiz erişim" değil "özellik hiç yok" doğrulanıyor.
    [Fact]
    public async Task Delete_ActionDoesNotExist()
    {
        await using var factory = new CustomWebApplicationFactory();
        var (trId, _) = await GetSeededLanguageIdsAsync(factory);

        var response = await factory.CreateClient(new() { AllowAutoRedirect = false })
            .AsAdmin()
            .PostAsync($"/Language/Delete/{trId}", new FormUrlEncodedContent([]));

        ((int)response.StatusCode).Should().Be(404);
    }

    // ADR-007'nin ertelenen kısmı — eksik çeviri raporu. Dil Yönetimi'nin geri kalanıyla aynı RBAC
    // (yalnızca Admin, salt-görüntüleme dahil hiçbir başka rolün erişimi yok).
    [Fact]
    public async Task Report_AllowedFor_Admin()
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin().GetAsync("/Language/Report");

        ((int)response.StatusCode).Should().Be(200);
    }

    [Theory]
    [InlineData("İçerik Editörü")]
    [InlineData("SEO Editörü")]
    [InlineData("Ürün Yöneticisi")]
    public async Task Report_DeniedFor_AllNonAdminRoles(string role)
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsRole(role).GetAsync("/Language/Report");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    public async Task Report_AnonymousAccess_IsDenied()
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsAnonymous().GetAsync("/Language/Report");

        ((int)response.StatusCode).Should().BeOneOf(401, 302, 403);
    }

    [Fact]
    public async Task Report_WithNoData_ShowsZeroTotal()
    {
        await using var factory = new CustomWebApplicationFactory();
        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin().GetAsync("/Language/Report");
        var body = await response.Content.ReadAsStringAsync();

        ((int)response.StatusCode).Should().Be(200);
        body.Should().Contain("Seçili filtrelerle eksik çeviri bulunmuyor.");
    }

    // Kesin kural (görev talimatı) — "Sadece tespit et ve raporla": rapor yalnızca hangi
    // (Kayıt, Dil, Alan) üçlüsünün eksik olduğunu listeler, hiçbir değer icat etmez/başka dilden
    // doldurmaz. Alan adları (Name/ShortDescription/...) İngilizce sabitler olduğu için Türkçe
    // karakter HTML-entity kodlaması riski yok (bkz. proje geneli test kuralı).
    [Fact]
    public async Task Report_ProductWithOnlyTrTranslation_ListsMissingEnglishFields()
    {
        await using var factory = new CustomWebApplicationFactory();
        var productCode = await SeedProductWithOnlyTrTranslationAsync(factory, "COVINT01RP");

        // type=Product filtresi olmadan, bu testin yardımcı kategorisinin (hiç çevirisi olmayan, 7 dil
        // x 5 alan = 35 satır) sayfa boyutunu (20) doldurup Ürün satırlarını 2. sayfaya itmesi riski var.
        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin().GetAsync("/Language/Report?type=Product");
        var body = await response.Content.ReadAsStringAsync();

        ((int)response.StatusCode).Should().Be(200);
        body.Should().Contain("Name");
        body.Should().Contain("ShortDescription");
        body.Should().Contain("SeoUrl");
        body.Should().Contain("Amazonit", "TR Name görüntü adı olarak kullanılmalı");
        productCode.Should().Be("COVINT01RP");
    }

    [Fact]
    public async Task Report_FilterByType_OnlyShowsMatchingModule()
    {
        await using var factory = new CustomWebApplicationFactory();
        await SeedProductWithOnlyTrTranslationAsync(factory, "COVINT02RP");

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Banners.Add(new Domain.Entities.Banner(null, null, 0));
            await db.SaveChangesAsync();
        }

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();
        var response = await client.GetAsync("/Language/Report?type=Banner");
        var body = await response.Content.ReadAsStringAsync();

        ((int)response.StatusCode).Should().Be(200);
        body.Should().NotContain("Amazonit", "type=Banner filtresiyle Ürün kaydı (Amazonit) alt listede görünmemeli");
    }

    // Filtre yalnızca alt listeyi daraltır — Ürün'ün EN için TAM OLARAK 6 alanı eksik (Name/
    // ShortDescription/LongDescription/SeoUrl/MetaTitle/MetaDescription, hiçbiri EN olarak girilmedi),
    // bu yüzden languageId=EN filtresiyle liste başlığı deterministik olarak "(6)" göstermeli — 7 dilin
    // TAMAMI (TR dahil, TR'de de LongDescription/SeoUrl/MetaTitle/MetaDescription eksik) filtresiz
    // toplamdan kesinlikle daha az.
    [Fact]
    public async Task Report_FilterByLanguage_NarrowsListToThatLanguageOnly()
    {
        await using var factory = new CustomWebApplicationFactory();
        await SeedProductWithOnlyTrTranslationAsync(factory, "COVINT03RP");
        var (_, enId) = await GetSeededLanguageIdsAsync(factory);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();

        var unfilteredResponse = await client.GetAsync("/Language/Report");
        var unfilteredBody = await unfilteredResponse.Content.ReadAsStringAsync();

        var filteredResponse = await client.GetAsync($"/Language/Report?type=Product&languageId={enId}");
        var filteredBody = await filteredResponse.Content.ReadAsStringAsync();

        ((int)filteredResponse.StatusCode).Should().Be(200);
        filteredBody.Should().Contain("Eksik Çeviriler (6)", "Ürün'ün EN için tam olarak 6 alanı eksik");
        unfilteredBody.Should().NotContain("Eksik Çeviriler (6)", "filtresiz liste 7 dilin tamamını kapsadığı için 6'dan büyük olmalı");
    }
}
