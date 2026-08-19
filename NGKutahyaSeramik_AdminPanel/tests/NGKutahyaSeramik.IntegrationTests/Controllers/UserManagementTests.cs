using FluentAssertions;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NGKutahyaSeramik.IntegrationTests.Authentication;
using NGKutahyaSeramik.IntegrationTests.Fixtures;
using NGKutahyaSeramik.IntegrationTests.Security;

namespace NGKutahyaSeramik.IntegrationTests.Controllers;

/// <summary>
/// Madde 30 — Kullanıcı Yönetimi (Task 16). Kendi-hesap ve son-aktif-Admin guardrail'leri ile
/// pasif-kullanıcı login davranışını gerçek HTTP pipeline'ı üzerinden doğrular.
///
/// Bu sınıf, diğer integration test sınıflarının aksine `IClassFixture&lt;CustomWebApplicationFactory&gt;`
/// PAYLAŞMIYOR — her test kendi `CustomWebApplicationFactory` örneğini (dolayısıyla kendi izole
/// SQLite bağlantısını) oluşturuyor. Sebep: `TestAuthHandler`'ın "mevcut oturumdaki kullanıcı"yı
/// sabit `"test-user-id"` claim'i ile temsil etmesi — kendi-hesap testlerinin bu id'de gerçek bir
/// `ApplicationUser` kaydı oluşturması gerekiyor, ve paylaşılan bir veritabanında bu testler arası
/// çakışacaktı (aynı Id ile ikinci bir CreateAsync başarısız olur). Per-test factory, sıfır ekstra
/// temizlik kodu gerektirmeden tam izolasyon sağlıyor.
/// </summary>
public class UserManagementTests
{
    private const string TestUserId = "test-user-id";
    private const string DefaultPassword = "Test-P@ssw0rd-123";

    private static async Task<ApplicationUser> CreateUserAsync(
        CustomWebApplicationFactory factory, string id, string email, bool isActive, string role, string? password = null)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }

        var user = new ApplicationUser { Id = id, UserName = email, Email = email, EmailConfirmed = true, IsActive = isActive };
        await userManager.CreateAsync(user, password ?? DefaultPassword);
        await userManager.AddToRoleAsync(user, role);

        return user;
    }

    private static async Task<ApplicationUser?> FindUserAsync(CustomWebApplicationFactory factory, string id)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return await userManager.FindByIdAsync(id);
    }

    [Fact]
    public async Task Deactivate_OwnAccount_IsRejected_ViaHttp()
    {
        await using var factory = new CustomWebApplicationFactory();
        await CreateUserAsync(factory, TestUserId, "self-deactivate@test.local", isActive: true, ApplicationRoles.Admin);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client, formUrl: "/User/Create", postUrl: $"/User/Deactivate/{TestUserId}", formValues: []);

        ((int)response.StatusCode).Should().Be(302, "RBAC izinli, yalnızca iş kuralı reddediyor — PRG her zaman gerçekleşir");
        (await FindUserAsync(factory, TestUserId))!.IsActive.Should().BeTrue("kendi hesabını pasifleştirme reddedilmeli");
    }

    [Fact]
    public async Task Delete_OwnAccount_IsRejected_ViaHttp()
    {
        await using var factory = new CustomWebApplicationFactory();
        await CreateUserAsync(factory, TestUserId, "self-delete@test.local", isActive: true, ApplicationRoles.Admin);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client, formUrl: "/User/Create", postUrl: $"/User/Delete/{TestUserId}", formValues: []);

        ((int)response.StatusCode).Should().Be(302);
        (await FindUserAsync(factory, TestUserId)).Should().NotBeNull("kendi hesabını silme reddedilmeli");
    }

    [Fact]
    public async Task Deactivate_LastActiveAdmin_IsRejected_ViaHttp()
    {
        await using var factory = new CustomWebApplicationFactory();
        // Hedef, "test-user-id" DEĞİL — kendi-hesap guardrail'inden ayrışsın, yalnızca son-aktif-Admin
        // korumasını izole test etsin. Aktör (test-user-id) TestAuthHandler ile Admin rolü claim'ine
        // sahip ama DB'de gerçek bir kayda karşılık gelmesi gerekmiyor (RBAC yalnızca claim'e bakar).
        var lastAdmin = await CreateUserAsync(factory, "lone-admin-id", "lone-admin@test.local", isActive: true, ApplicationRoles.Admin);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client, formUrl: "/User/Create", postUrl: $"/User/Deactivate/{lastAdmin.Id}", formValues: []);

        ((int)response.StatusCode).Should().Be(302);
        (await FindUserAsync(factory, lastAdmin.Id))!.IsActive.Should().BeTrue("sistemdeki son aktif Admin pasifleştirilemez");
    }

    [Fact]
    public async Task Delete_LastActiveAdmin_IsRejected_ViaHttp()
    {
        await using var factory = new CustomWebApplicationFactory();
        var lastAdmin = await CreateUserAsync(factory, "lone-admin-id", "lone-admin-del@test.local", isActive: true, ApplicationRoles.Admin);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client, formUrl: "/User/Create", postUrl: $"/User/Delete/{lastAdmin.Id}", formValues: []);

        ((int)response.StatusCode).Should().Be(302);
        (await FindUserAsync(factory, lastAdmin.Id)).Should().NotBeNull("sistemdeki son aktif Admin silinemez");
    }

    [Fact]
    public async Task Edit_RemovingLastActiveAdminRole_IsRejected_ViaHttp()
    {
        await using var factory = new CustomWebApplicationFactory();
        var lastAdmin = await CreateUserAsync(factory, "lone-admin-id", "lone-admin-role@test.local", isActive: true, ApplicationRoles.Admin);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: "/User/Create",
            postUrl: $"/User/Edit/{lastAdmin.Id}",
            formValues: new Dictionary<string, string>
            {
                ["IsActive"] = "true",
                ["Role"] = ApplicationRoles.ContentEditor
            });

        // Edit, Deactivate/Delete/Activate'in aksine başarısızlıkta Index'e yönlenmez — Blog/News/
        // Product Edit deseniyle tutarlı: ModelState hatasıyla form 200'le tekrar render edilir.
        ((int)response.StatusCode).Should().Be(200, "guardrail reddi Edit formunu hata mesajıyla tekrar render etmeli");
        var reloaded = await FindUserAsync(factory, lastAdmin.Id);

        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        (await userManager.IsInRoleAsync(reloaded!, ApplicationRoles.Admin)).Should().BeTrue(
            "sistemdeki son aktif Admin'in Admin rolü kaldırılamaz");
    }

    [Fact]
    public async Task Login_InactiveUser_WithCorrectPassword_IsRejected()
    {
        await using var factory = new CustomWebApplicationFactory();
        var email = $"inactive-{Guid.NewGuid():N}@test.local";
        await CreateUserAsync(factory, Guid.NewGuid().ToString(), email, isActive: false, ApplicationRoles.ContentEditor);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAnonymous();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: "/Account/Login",
            postUrl: "/Account/Login",
            formValues: new Dictionary<string, string> { ["Email"] = email, ["Password"] = DefaultPassword });

        // Login başarısız → PRG yok, form 200 ile (genel hata mesajıyla) tekrar render edilir.
        ((int)response.StatusCode).Should().Be(200, "pasif kullanıcı doğru parola girse bile giriş yapamamalı");
    }

    [Fact]
    public async Task Login_ActiveUser_WithCorrectPassword_Succeeds()
    {
        await using var factory = new CustomWebApplicationFactory();
        var email = $"active-{Guid.NewGuid():N}@test.local";
        await CreateUserAsync(factory, Guid.NewGuid().ToString(), email, isActive: true, ApplicationRoles.ContentEditor);

        var client = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAnonymous();
        var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
            client,
            formUrl: "/Account/Login",
            postUrl: "/Account/Login",
            formValues: new Dictionary<string, string> { ["Email"] = email, ["Password"] = DefaultPassword });

        ((int)response.StatusCode).Should().Be(302, "aktif kullanıcı doğru parolayla giriş yapabilmeli");
    }
}
