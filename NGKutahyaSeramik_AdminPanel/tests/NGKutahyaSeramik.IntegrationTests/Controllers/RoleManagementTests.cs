using FluentAssertions;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NGKutahyaSeramik.IntegrationTests.Authentication;
using NGKutahyaSeramik.IntegrationTests.Fixtures;
using Xunit;

namespace NGKutahyaSeramik.IntegrationTests.Controllers;

/// <summary>
/// Madde 30 — Kullanıcı/Rol Yönetimi (Task 17, salt-okunur `RoleController`). Program.cs'in kendi
/// startup seed'i (`IdentitySeeder.SeedRolesAsync`, `seedOnStartup=true`) `CustomWebApplicationFactory`
/// içinde de çalıştığı için 4 sabit rol her testte DB'de hazır — ayrıca seed edilmesine gerek yok.
/// Diğer RBAC test sınıflarıyla (`RbacTests`) aynı paylaşılan fixture deseni kullanılıyor — bu
/// controller'da kendi-hesap/guardrail testi olmadığı için `UserManagementTests`'teki per-test
/// factory izolasyonuna gerek yok.
/// </summary>
public class RoleManagementTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RoleManagementTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient Client() => _factory.CreateClient(new() { AllowAutoRedirect = false });

    private async Task CreateUserInRoleAsync(string email, string role)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, IsActive = true };
        await userManager.CreateAsync(user, "Test-P@ssw0rd-123");
        await userManager.AddToRoleAsync(user, role);
    }

    // 1. Anonymous /Role erişimi login'e yönlenir
    [Fact]
    public async Task Anonymous_Index_IsDenied()
    {
        var response = await Client().AsAnonymous().GetAsync("/Role");

        ((int)response.StatusCode).Should().BeOneOf(401, 302, 403);
    }

    // 2. Anonymous /Role/Details/{role} erişimi login'e yönlenir
    [Fact]
    public async Task Anonymous_Details_IsDenied()
    {
        var response = await Client().AsAnonymous().GetAsync($"/Role/Details/{ApplicationRoles.Admin}");

        ((int)response.StatusCode).Should().BeOneOf(401, 302, 403);
    }

    // 3. Admin Index'e erişebilir
    [Fact]
    public async Task Admin_Index_IsAllowed()
    {
        var response = await Client().AsAdmin().GetAsync("/Role");

        ((int)response.StatusCode).Should().Be(200);
    }

    // 4. Admin Details'e erişebilir
    [Fact]
    public async Task Admin_Details_IsAllowed()
    {
        var response = await Client().AsAdmin()
            .GetAsync($"/Role/Details/{Uri.EscapeDataString(ApplicationRoles.ContentEditor)}");

        ((int)response.StatusCode).Should().Be(200);
    }

    // 5-7. ContentEditor/SeoEditor/ProductManager erişemez
    [Theory]
    [InlineData("İçerik Editörü")]
    [InlineData("SEO Editörü")]
    [InlineData("Ürün Yöneticisi")]
    public async Task NonAdminRoles_Index_IsDenied(string role)
    {
        var response = await Client().AsRole(role).GetAsync("/Role");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    // 8. Rolsüz authenticated kullanıcı erişemez
    [Fact]
    public async Task AuthenticatedWithoutRole_Index_IsDenied()
    {
        var response = await Client().AsAuthenticatedWithoutRole().GetAsync("/Role");

        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    // 9. Geçersiz rol adı 404 döner
    [Fact]
    public async Task Admin_Details_UnknownRole_ReturnsNotFound()
    {
        var response = await Client().AsAdmin().GetAsync("/Role/Details/BilinmeyenRol");

        ((int)response.StatusCode).Should().Be(404);
    }

    // 10. Rol detayında kullanıcı listesi render edilir
    [Fact]
    public async Task Admin_Details_RendersAssignedUserList()
    {
        var email = $"roledetail-{Guid.NewGuid():N}@test.local";
        await CreateUserInRoleAsync(email, ApplicationRoles.SeoEditor);

        var response = await Client().AsAdmin()
            .GetAsync($"/Role/Details/{Uri.EscapeDataString(ApplicationRoles.SeoEditor)}");
        var body = await response.Content.ReadAsStringAsync();

        ((int)response.StatusCode).Should().Be(200);
        body.Should().Contain(email);
    }

    // 11. Navigation linki yalnız Admin için görünür
    [Fact]
    public async Task Navigation_RoleLink_OnlyVisibleForAdmin()
    {
        var adminBody = await (await Client().AsAdmin().GetAsync("/Home")).Content.ReadAsStringAsync();
        var editorBody = await (await Client().AsContentEditor().GetAsync("/Home")).Content.ReadAsStringAsync();

        adminBody.Should().Contain("Rol Yönetimi");
        editorBody.Should().NotContain("Rol Yönetimi");
    }

    // 12. Role Management endpoint'lerinde POST action bulunmaz
    [Theory]
    [InlineData("/Role")]
    [InlineData("/Role/Details/Admin")]
    public async Task Role_Endpoints_HaveNoPostAction(string url)
    {
        var response = await Client().AsAdmin().PostAsync(url, new FormUrlEncodedContent([]));

        ((int)response.StatusCode).Should().BeOneOf(404, 405);
    }
}
