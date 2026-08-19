using Application.Roles;
using FluentAssertions;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NGKutahyaSeramik.UnitTests.Common;
using Xunit;

namespace NGKutahyaSeramik.UnitTests.Services;

/// <summary>
/// Madde 30 — Kullanıcı/Rol Yönetimi (Task 17, salt-okunur). `IdentityTestHost` (gerçek AddIdentity+
/// SQLite grafiği) kullanılıyor. Her test kendi rol/kullanıcı seed'ini `SeedRolesAsync`/
/// `CreateUserInRoleAsync` ile açıkça yapar — `UserManagementServiceTests`'in aksine ortak bir
/// "4 rolü baştan seed et" adımı YOK, çünkü #14 senaryosu (eksik seed rolü) kasıtlı olarak eksik
/// seed test ediyor.
/// </summary>
public class RoleManagementServiceTests : IAsyncLifetime
{
    private IdentityTestHost _host = null!;
    private IServiceScope _scope = null!;
    private UserManager<ApplicationUser> _userManager = null!;
    private RoleManager<IdentityRole> _roleManager = null!;
    private RoleManagementService _sut = null!;

    public Task InitializeAsync()
    {
        _host = IdentityTestHost.Create();
        _scope = _host.Services.CreateScope();
        _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        _roleManager = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        _sut = new RoleManagementService(_roleManager, _userManager);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        _host.Dispose();
        return Task.CompletedTask;
    }

    private async Task SeedRolesAsync(params string[] roles)
    {
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private async Task<ApplicationUser> CreateUserInRoleAsync(string email, string role, bool isActive = true)
    {
        await SeedRolesAsync(role);
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, IsActive = isActive };
        await _userManager.CreateAsync(user, "Test-P@ssw0rd-123");
        await _userManager.AddToRoleAsync(user, role);
        return user;
    }

    // 1. Dört sabit rol doğru listelenir
    [Fact]
    public async Task GetAllAsync_ListsExactlyFourFixedRoles()
    {
        await SeedRolesAsync(ApplicationRoles.All.ToArray());

        var roles = await _sut.GetAllAsync();

        roles.Select(r => r.Name).Should().BeEquivalentTo(ApplicationRoles.All);
    }

    // 2. Beklenmeyen DB rolü sistem rolü olarak listelenmez
    [Fact]
    public async Task GetAllAsync_DoesNotListUnexpectedDbRole()
    {
        await SeedRolesAsync(ApplicationRoles.All.ToArray());
        await _roleManager.CreateAsync(new IdentityRole("Beklenmeyen Rol"));

        var roles = await _sut.GetAllAsync();

        roles.Should().HaveCount(4);
        roles.Select(r => r.Name).Should().NotContain("Beklenmeyen Rol");
    }

    // 3. Rol kullanıcı sayıları doğru hesaplanır
    [Fact]
    public async Task GetAllAsync_CalculatesUserCountsCorrectly()
    {
        await CreateUserInRoleAsync("u1@test.local", ApplicationRoles.ContentEditor);
        await CreateUserInRoleAsync("u2@test.local", ApplicationRoles.ContentEditor);

        var roles = await _sut.GetAllAsync();

        roles.Single(r => r.Name == ApplicationRoles.ContentEditor).TotalUserCount.Should().Be(2);
    }

    // 4. Aktif kullanıcı sayısı doğru hesaplanır
    [Fact]
    public async Task GetAllAsync_CalculatesActiveUserCountCorrectly()
    {
        await CreateUserInRoleAsync("active1@test.local", ApplicationRoles.SeoEditor, isActive: true);
        await CreateUserInRoleAsync("active2@test.local", ApplicationRoles.SeoEditor, isActive: true);
        await CreateUserInRoleAsync("inactive1@test.local", ApplicationRoles.SeoEditor, isActive: false);

        var roles = await _sut.GetAllAsync();

        roles.Single(r => r.Name == ApplicationRoles.SeoEditor).ActiveUserCount.Should().Be(2);
    }

    // 5. Pasif kullanıcı sayıya yalnız toplamda dahil edilir
    [Fact]
    public async Task GetAllAsync_InactiveUser_CountedOnlyInTotal_NotActive()
    {
        await CreateUserInRoleAsync("onlyinactive@test.local", ApplicationRoles.ProductManager, isActive: false);

        var role = (await _sut.GetAllAsync()).Single(r => r.Name == ApplicationRoles.ProductManager);

        role.TotalUserCount.Should().Be(1);
        role.ActiveUserCount.Should().Be(0);
    }

    // 6. Role atanmış kullanıcılar doğru gelir
    [Fact]
    public async Task GetByNameAsync_ReturnsCorrectAssignedUsers()
    {
        var member = await CreateUserInRoleAsync("member1@test.local", ApplicationRoles.ContentEditor);
        await CreateUserInRoleAsync("other@test.local", ApplicationRoles.Admin);

        var detail = await _sut.GetByNameAsync(ApplicationRoles.ContentEditor);

        detail!.Users.Should().ContainSingle(u => u.Id == member.Id);
    }

    // 7. Kullanıcılar email'e göre sıralanır
    [Fact]
    public async Task GetByNameAsync_UsersAreSortedByEmail()
    {
        await CreateUserInRoleAsync("zeta@test.local", ApplicationRoles.Admin);
        await CreateUserInRoleAsync("alpha@test.local", ApplicationRoles.Admin);
        await CreateUserInRoleAsync("mike@test.local", ApplicationRoles.Admin);

        var detail = await _sut.GetByNameAsync(ApplicationRoles.Admin);

        detail!.Users.Select(u => u.Email).Should().BeInAscendingOrder();
    }

    // 8. Rol bulunamazsa uygun sonuç döner
    [Fact]
    public async Task GetByNameAsync_UnknownRole_ReturnsNull()
    {
        (await _sut.GetByNameAsync("Bilinmeyen Rol")).Should().BeNull();
    }

    // 9. Yetki matrisi doğru role doğru modülleri verir
    [Fact]
    public async Task GetByNameAsync_PermissionMatrix_MatchesKnownRbac()
    {
        var detail = await _sut.GetByNameAsync(ApplicationRoles.ProductManager);

        detail!.Permissions.Single(p => p.ModuleName == "Ürün Yönetimi").AccessLevel.Should().Be(RoleAccessLevel.Full);
        detail.Permissions.Single(p => p.ModuleName == "Blog Yönetimi").AccessLevel.Should().Be(RoleAccessLevel.None);
        detail.Permissions.Single(p => p.ModuleName == "SEO Yönetimi").AccessLevel.Should().Be(RoleAccessLevel.None);
    }

    // Backlog #4 — SEO Yönetimi artık uygulandı: Admin+SEO Editörü Full, diğer roller None.
    [Fact]
    public async Task GetByNameAsync_PermissionMatrix_SeoManagement_OnlyFullForAdminAndSeoEditor()
    {
        var adminDetail = await _sut.GetByNameAsync(ApplicationRoles.Admin);
        var seoDetail = await _sut.GetByNameAsync(ApplicationRoles.SeoEditor);
        var contentEditorDetail = await _sut.GetByNameAsync(ApplicationRoles.ContentEditor);

        adminDetail!.Permissions.Single(p => p.ModuleName == "SEO Yönetimi").AccessLevel.Should().Be(RoleAccessLevel.Full);
        seoDetail!.Permissions.Single(p => p.ModuleName == "SEO Yönetimi").AccessLevel.Should().Be(RoleAccessLevel.Full);
        contentEditorDetail!.Permissions.Single(p => p.ModuleName == "SEO Yönetimi").AccessLevel.Should().Be(RoleAccessLevel.None);
    }

    // 8B. Backlog #3 — Dil Yönetimi artık uygulandı: yalnızca Admin Full, diğer roller None.
    [Fact]
    public async Task GetByNameAsync_PermissionMatrix_LanguageManagement_OnlyFullForAdmin()
    {
        var adminDetail = await _sut.GetByNameAsync(ApplicationRoles.Admin);
        var pmDetail = await _sut.GetByNameAsync(ApplicationRoles.ProductManager);

        adminDetail!.Permissions.Single(p => p.ModuleName == "Dil Yönetimi").AccessLevel.Should().Be(RoleAccessLevel.Full);
        pmDetail!.Permissions.Single(p => p.ModuleName == "Dil Yönetimi").AccessLevel.Should().Be(RoleAccessLevel.None);
    }

    // 9A. Backlog #23 — İçerik Editörü ve SEO Editörü, Ürün Yönetimi'nde artık PartialFields (önceden None).
    [Fact]
    public async Task GetByNameAsync_PermissionMatrix_ProductManagement_ShowsPartialFieldsForContentAndSeoEditor()
    {
        var contentEditorDetail = await _sut.GetByNameAsync(ApplicationRoles.ContentEditor);
        var seoEditorDetail = await _sut.GetByNameAsync(ApplicationRoles.SeoEditor);

        contentEditorDetail!.Permissions.Single(p => p.ModuleName == "Ürün Yönetimi").AccessLevel
            .Should().Be(RoleAccessLevel.PartialFields);
        seoEditorDetail!.Permissions.Single(p => p.ModuleName == "Ürün Yönetimi").AccessLevel
            .Should().Be(RoleAccessLevel.PartialFields);
    }

    // 9B. Backlog #23 — SEO Editörü, Sayfa Yönetimi'nde artık PartialFields (önceden ViewOnly);
    // İçerik Editörü Sayfa Yönetimi'nde hâlâ Full (değişmedi).
    [Fact]
    public async Task GetByNameAsync_PermissionMatrix_PageManagement_ShowsPartialFieldsForSeoEditor_FullForContentEditor()
    {
        var contentEditorDetail = await _sut.GetByNameAsync(ApplicationRoles.ContentEditor);
        var seoEditorDetail = await _sut.GetByNameAsync(ApplicationRoles.SeoEditor);

        contentEditorDetail!.Permissions.Single(p => p.ModuleName == "Sayfa Yönetimi").AccessLevel
            .Should().Be(RoleAccessLevel.Full);
        seoEditorDetail!.Permissions.Single(p => p.ModuleName == "Sayfa Yönetimi").AccessLevel
            .Should().Be(RoleAccessLevel.PartialFields);
    }

    // 9C. Regresyon — Admin/Ürün Yöneticisi hâlâ Full (değişmedi).
    [Fact]
    public async Task GetByNameAsync_PermissionMatrix_ProductManagement_AdminAndProductManagerStayFull()
    {
        var adminDetail = await _sut.GetByNameAsync(ApplicationRoles.Admin);
        var pmDetail = await _sut.GetByNameAsync(ApplicationRoles.ProductManager);

        adminDetail!.Permissions.Single(p => p.ModuleName == "Ürün Yönetimi").AccessLevel.Should().Be(RoleAccessLevel.Full);
        pmDetail!.Permissions.Single(p => p.ModuleName == "Ürün Yönetimi").AccessLevel.Should().Be(RoleAccessLevel.Full);
    }

    // 10. User Management yalnız Admin için görünür
    [Fact]
    public async Task GetByNameAsync_UserRoleManagementModule_OnlyFullForAdmin()
    {
        var adminDetail = await _sut.GetByNameAsync(ApplicationRoles.Admin);
        var editorDetail = await _sut.GetByNameAsync(ApplicationRoles.ContentEditor);

        adminDetail!.Permissions.Single(p => p.ModuleName == "Kullanıcı/Rol Yönetimi").AccessLevel.Should().Be(RoleAccessLevel.Full);
        editorDetail!.Permissions.Single(p => p.ModuleName == "Kullanıcı/Rol Yönetimi").AccessLevel.Should().Be(RoleAccessLevel.None);
    }

    // 11. Admin sistem rolü korunur (yalnızca görüntülenir — silme/yeniden adlandırma action'ı hiç yok)
    [Fact]
    public async Task GetAllAsync_AdminRole_AlwaysListed()
    {
        await SeedRolesAsync(ApplicationRoles.All.ToArray());

        var roles = await _sut.GetAllAsync();

        roles.Should().Contain(r => r.Name == ApplicationRoles.Admin);
    }

    // 12. Tek rol kuralıyla uyumlu veri okunur
    [Fact]
    public async Task GetAllAsync_SingleRoleUser_CountedOnlyUnderAssignedRole()
    {
        await CreateUserInRoleAsync("single@test.local", ApplicationRoles.SeoEditor);

        var roles = await _sut.GetAllAsync();

        roles.Single(r => r.Name == ApplicationRoles.SeoEditor).TotalUserCount.Should().Be(1);
        roles.Single(r => r.Name == ApplicationRoles.Admin).TotalUserCount.Should().Be(0);
        roles.Single(r => r.Name == ApplicationRoles.ContentEditor).TotalUserCount.Should().Be(0);
        roles.Single(r => r.Name == ApplicationRoles.ProductManager).TotalUserCount.Should().Be(0);
    }

    // 13. Boş kullanıcı listesi doğru işlenir
    [Fact]
    public async Task GetByNameAsync_NoUsersAssigned_ReturnsEmptyListWithZeroCounts()
    {
        await SeedRolesAsync(ApplicationRoles.Admin);

        var detail = await _sut.GetByNameAsync(ApplicationRoles.Admin);

        detail!.Users.Should().BeEmpty();
        detail.ActiveUserCount.Should().Be(0);
        detail.TotalUserCount.Should().Be(0);
    }

    // 14. Identity'de eksik seed rolü varsa kontrollü davranılır
    [Fact]
    public async Task GetAllAsync_MissingSeedRole_HandledGracefully_NoException()
    {
        // ProductManager kasıtlı olarak seed edilmedi.
        await SeedRolesAsync(ApplicationRoles.Admin, ApplicationRoles.ContentEditor, ApplicationRoles.SeoEditor);

        var act = async () => await _sut.GetAllAsync();

        var roles = await act.Should().NotThrowAsync();
        roles.Which.Should().HaveCount(4);
        roles.Which.Single(r => r.Name == ApplicationRoles.ProductManager).TotalUserCount.Should().Be(0);
    }

    [Fact]
    public async Task GetByNameAsync_MissingSeedRole_ReturnsDetailWithZeroCounts_NotNull()
    {
        // ProductManager hiç seed edilmedi (AspNetRoles'ta karşılığı yok) — yine de ApplicationRoles.All
        // içinde olduğu için sistem rolü olarak tanınmalı, 0 sayaçla, hatasız.
        var detail = await _sut.GetByNameAsync(ApplicationRoles.ProductManager);

        detail.Should().NotBeNull();
        detail!.Users.Should().BeEmpty();
        detail.ActiveUserCount.Should().Be(0);
        detail.TotalUserCount.Should().Be(0);
        detail.Permissions.Should().NotBeEmpty();
    }
}
