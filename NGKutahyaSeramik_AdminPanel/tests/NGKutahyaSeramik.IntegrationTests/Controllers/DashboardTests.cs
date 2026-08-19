using FluentAssertions;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NGKutahyaSeramik.IntegrationTests.Authentication;
using NGKutahyaSeramik.IntegrationTests.Fixtures;
using NGKutahyaSeramik.UnitTests.Factories;
using Xunit;

namespace NGKutahyaSeramik.IntegrationTests.Controllers;

/// <summary>
/// Madde 17.2 modül #1 (Dashboard, Task 18). RBAC/routing testleri paylaşılan
/// `IClassFixture&lt;CustomWebApplicationFactory&gt;` kullanır (`RbacTests`/`RoleManagementTests` ile
/// aynı desen). Belirli veriye bağımlı testler (sayaç doğruluğu, sıralama, boş-veri davranışı) kendi
/// izole `CustomWebApplicationFactory` örneğini kullanır (TESTING.md §7B deseni) — paylaşılan DB'de
/// testler arası veri sızıntısı/sıra bağımlılığı riskini önlemek için.
/// </summary>
public class DashboardTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DashboardTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient Client() => _factory.CreateClient(new() { AllowAutoRedirect = false });

    private static async Task SeedProductAsync(CustomWebApplicationFactory factory, string productCode, DateTime createdAt)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // DisplayOrder her tabloda benzersiz olmak zorunda (UX_*_DisplayOrder index'leri) — bu helper
        // aynı test içinde birden çok kez çağrılabildiği için sabit bir değer yerine mevcut maksimumun
        // bir fazlası kullanılır.
        var categoryDisplayOrder = (await db.Categories.MaxAsync(c => (int?)c.DisplayOrder) ?? 0) + 1;
        var collectionDisplayOrder = (await db.Collections.MaxAsync(c => (int?)c.DisplayOrder) ?? 0) + 1;
        var category = CategoryFactory.CreateRoot(displayOrder: categoryDisplayOrder);
        var collection = CollectionFactory.CreateValid(displayOrder: collectionDisplayOrder);
        db.Categories.Add(category);
        db.Collections.Add(collection);
        await db.SaveChangesAsync();

        var productDisplayOrder = (await db.Products.MaxAsync(p => (int?)p.DisplayOrder) ?? 0) + 1;
        var product = ProductFactory.CreateValid(productCode, category.Id, collection.Id, displayOrder: productDisplayOrder);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        db.Entry(product).Property("CreatedAt").CurrentValue = createdAt;
        await db.SaveChangesAsync();
    }

    private static async Task SeedFormAsync(CustomWebApplicationFactory factory, string fullName, DateTime createdAt)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var form = FormSubmissionFactory.CreateContact(fullName);
        db.FormSubmissions.Add(form);
        await db.SaveChangesAsync();
        db.Entry(form).Property("CreatedAt").CurrentValue = createdAt;
        await db.SaveChangesAsync();
    }

    // 1. Anonim kullanıcı Dashboard'a erişemez
    [Fact]
    public async Task Anonymous_Index_IsDenied()
    {
        var response = await Client().AsAnonymous().GetAsync("/Home");

        ((int)response.StatusCode).Should().BeOneOf(401, 302, 403);
    }

    // 2. Admin erişebilir
    [Fact]
    public async Task Admin_Index_IsAllowed()
    {
        var response = await Client().AsAdmin().GetAsync("/Home");

        ((int)response.StatusCode).Should().Be(200);
    }

    // 3. ContentEditor erişebilir
    [Fact]
    public async Task ContentEditor_Index_IsAllowed()
    {
        var response = await Client().AsContentEditor().GetAsync("/Home");

        ((int)response.StatusCode).Should().Be(200);
    }

    // 4. SeoEditor erişebilir
    [Fact]
    public async Task SeoEditor_Index_IsAllowed()
    {
        var response = await Client().AsSeoEditor().GetAsync("/Home");

        ((int)response.StatusCode).Should().Be(200);
    }

    // 5. ProductManager erişebilir
    [Fact]
    public async Task ProductManager_Index_IsAllowed()
    {
        var response = await Client().AsProductManager().GetAsync("/Home");

        ((int)response.StatusCode).Should().Be(200);
    }

    // 6. Rolsüz authenticated kullanıcı da erişebilir — HomeController.Index [Authorize] kullanıyor,
    // [Authorize(Roles=...)] DEĞİL (Task 2.1'den beri değişmedi, Task 18 analiziyle de bilinçli
    // olarak korundu: Dashboard'un mevcut erişim kuralı rol şartı taşımıyor, yalnızca authenticated
    // olmak yeterli — diğer tüm modüllerin [Authorize(Roles=...)] deseninden farklı, kasıtlı).
    [Fact]
    public async Task AuthenticatedWithoutRole_Index_IsAllowed()
    {
        var response = await Client().AsAuthenticatedWithoutRole().GetAsync("/Home");

        ((int)response.StatusCode).Should().Be(200);
    }

    // 7. Dashboard başarılı şekilde render edilir (beklenen yapı içerir)
    [Fact]
    public async Task Admin_Index_RendersExpectedSections()
    {
        var response = await Client().AsAdmin().GetAsync("/Home");
        var body = await response.Content.ReadAsStringAsync();

        ((int)response.StatusCode).Should().Be(200);
        body.Should().Contain("Özet");
        body.Should().Contain("Son Ürünler");
    }

    // 8. Dashboard gerçek sayaçları gösterir
    [Fact]
    public async Task Admin_Index_ShowsRealCounts()
    {
        await using var factory = new CustomWebApplicationFactory();
        await SeedProductAsync(factory, "COUNT001RP", DateTime.UtcNow);
        await SeedProductAsync(factory, "COUNT002RP", DateTime.UtcNow);

        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin().GetAsync("/Home");
        var body = await response.Content.ReadAsStringAsync();

        ((int)response.StatusCode).Should().Be(200);
        body.Should().Contain("Aktif: 2");
    }

    // 9. Veri yokken Dashboard hata vermez
    [Fact]
    public async Task Admin_Index_WithNoData_DoesNotThrow()
    {
        await using var factory = new CustomWebApplicationFactory();

        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin().GetAsync("/Home");
        var body = await response.Content.ReadAsStringAsync();

        ((int)response.StatusCode).Should().Be(200);
        body.Should().Contain("Henüz ürün bulunmuyor.");
    }

    // 10. Son ürünler doğru sırada gösterilir
    [Fact]
    public async Task Admin_Index_RecentProducts_InCorrectOrder()
    {
        await using var factory = new CustomWebApplicationFactory();
        var now = DateTime.UtcNow;
        await SeedProductAsync(factory, "OLDER001RP", now.AddDays(-2));
        await SeedProductAsync(factory, "NEWER001RP", now);

        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin().GetAsync("/Home");
        var body = await response.Content.ReadAsStringAsync();

        var newerIndex = body.IndexOf("NEWER001RP", StringComparison.Ordinal);
        var olderIndex = body.IndexOf("OLDER001RP", StringComparison.Ordinal);

        newerIndex.Should().BeGreaterThan(-1);
        olderIndex.Should().BeGreaterThan(-1);
        newerIndex.Should().BeLessThan(olderIndex, "en yeni ürün listede daha önce görünmeli");
    }

    // 11. Son formlar doğru sırada gösterilir
    [Fact]
    public async Task Admin_Index_RecentForms_InCorrectOrder()
    {
        await using var factory = new CustomWebApplicationFactory();
        var now = DateTime.UtcNow;
        await SeedFormAsync(factory, "Eski Basvuran", now.AddDays(-2));
        await SeedFormAsync(factory, "Yeni Basvuran", now);

        var response = await factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin().GetAsync("/Home");
        var body = await response.Content.ReadAsStringAsync();

        var newerIndex = body.IndexOf("Yeni Basvuran", StringComparison.Ordinal);
        var olderIndex = body.IndexOf("Eski Basvuran", StringComparison.Ordinal);

        newerIndex.Should().BeGreaterThan(-1);
        olderIndex.Should().BeGreaterThan(-1);
        newerIndex.Should().BeLessThan(olderIndex, "en yeni form listede daha önce görünmeli");
    }

    // 12. Dashboard yalnızca GET action içerir
    [Fact]
    public async Task Index_HasNoPostAction()
    {
        var response = await Client().AsAdmin().PostAsync("/Home", new FormUrlEncodedContent([]));

        ((int)response.StatusCode).Should().BeOneOf(404, 405);
    }

    // 13. Dashboard linkleri mevcut authorization yapısıyla uyumlu
    [Fact]
    public async Task SeoEditor_Index_DoesNotRenderFormSubmissionsSection()
    {
        // SeoEditor'ün FormSubmissionController.ViewRoles'e hiç erişimi yok (Admin+İçerik Editörü) —
        // Dashboard'daki "Son Form Başvuruları" bölümü bu rol için hiç render edilmemeli.
        var response = await Client().AsSeoEditor().GetAsync("/Home");
        var body = await response.Content.ReadAsStringAsync();

        ((int)response.StatusCode).Should().Be(200);
        body.Should().NotContain("Son Form Başvuruları");
    }

    [Fact]
    public async Task ContentEditor_Index_RendersFormSubmissionsSection()
    {
        var response = await Client().AsContentEditor().GetAsync("/Home");
        var body = await response.Content.ReadAsStringAsync();

        ((int)response.StatusCode).Should().Be(200);
        body.Should().Contain("Son Form Başvuruları");
    }

    [Theory]
    [InlineData("SeoEditor")]
    [InlineData("ContentEditor")]
    [InlineData("ProductManager")]
    public async Task NonAdmin_Index_DoesNotShowMissingTranslationCard(string role)
    {
        var client = Client();
        var response = role switch
        {
            "SeoEditor" => await client.AsSeoEditor().GetAsync("/Home"),
            "ContentEditor" => await client.AsContentEditor().GetAsync("/Home"),
            _ => await client.AsProductManager().GetAsync("/Home")
        };
        var body = await response.Content.ReadAsStringAsync();

        ((int)response.StatusCode).Should().Be(200);
        body.Should().NotContain("/Language/Report", "Dil Yönetimi Admin-only olduğu için diğer roller bu linki görmemeli");
    }
}
