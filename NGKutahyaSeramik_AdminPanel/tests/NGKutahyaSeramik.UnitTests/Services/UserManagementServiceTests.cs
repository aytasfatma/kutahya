using Application.Users;
using FluentAssertions;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NGKutahyaSeramik.UnitTests.Common;
using Xunit;

namespace NGKutahyaSeramik.UnitTests.Services;

/// <summary>
/// Madde 30 — Kullanıcı Yönetimi (Task 16 / Task 16B revizyonu). `IdentityTestHost` (gerçek
/// AddIdentity+SQLite grafiği — UserManager/RoleManager salt Moq ile anlamlı test edilemez,
/// IdentitySeederTests'in kanıtladığı desenle birebir) kullanılıyor. Her test önce 4 rolü seed eder
/// (roller olmadan AddToRoleAsync başarısız olur) — admin kullanıcı seed edilmez, testler kendi
/// kullanıcılarını üretir. Revizyon: her kullanıcı tam olarak bir role sahip (CreateUserRequest/
/// UpdateUserRequest.Role tekil string), Email/UserName UpdateAsync ile değiştirilemez.
/// </summary>
public class UserManagementServiceTests : IAsyncLifetime
{
    private IdentityTestHost _host = null!;
    private IServiceScope _scope = null!;
    private UserManager<ApplicationUser> _userManager = null!;
    private RoleManager<IdentityRole> _roleManager = null!;
    private UserManagementService _sut = null!;

    public async Task InitializeAsync()
    {
        _host = IdentityTestHost.Create();
        _scope = _host.Services.CreateScope();
        _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        _roleManager = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = _scope.ServiceProvider.GetRequiredService<IConfiguration>();
        _sut = new UserManagementService(_userManager, _roleManager, configuration);

        foreach (var role in ApplicationRoles.All)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        _host.Dispose();
        return Task.CompletedTask;
    }

    private const string ValidPassword = "Test-P@ssw0rd-123";

    private static CreateUserRequest ValidCreateRequest(string email = "user@test.local", string role = ApplicationRoles.ContentEditor) => new()
    {
        Email = email,
        Password = ValidPassword,
        Role = role,
        IsActive = true
    };

    private async Task<UserDto> CreateAndGetAsync(string email = "user@test.local", string role = ApplicationRoles.ContentEditor)
    {
        var result = await _sut.CreateAsync(ValidCreateRequest(email, role));
        result.Succeeded.Should().BeTrue(result.ErrorMessage);
        return (await _sut.GetAllAsync()).Single(u => u.Email == email);
    }

    // 1. Geçerli kullanıcı oluşturma
    [Fact]
    public async Task CreateAsync_WithValidData_Succeeds()
    {
        var result = await _sut.CreateAsync(ValidCreateRequest());

        result.Succeeded.Should().BeTrue();
    }

    // 2. Email ve UserName aynı atanıyor
    [Fact]
    public async Task CreateAsync_SetsEmailAndUserNameToSameValue()
    {
        await _sut.CreateAsync(ValidCreateRequest("same@test.local"));

        var user = await _userManager.FindByEmailAsync("same@test.local");
        user!.UserName.Should().Be("same@test.local");
        user.Email.Should().Be("same@test.local");
    }

    // 3. EmailConfirmed=true
    [Fact]
    public async Task CreateAsync_SetsEmailConfirmedTrue()
    {
        var dto = await CreateAndGetAsync();

        dto.EmailConfirmed.Should().BeTrue();
    }

    // 4. Varsayılan IsActive=true
    [Fact]
    public async Task CreateAsync_DefaultsIsActiveTrue()
    {
        var dto = await CreateAndGetAsync();

        dto.IsActive.Should().BeTrue();
    }

    // 5. Geçersiz email reddi
    [Fact]
    public async Task CreateAsync_WithInvalidEmail_IsRejected()
    {
        var result = await _sut.CreateAsync(ValidCreateRequest("not-an-email"));

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("e-posta");
    }

    // 6. Duplicate email reddi
    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_IsRejected()
    {
        await _sut.CreateAsync(ValidCreateRequest("dup@test.local"));

        var result = await _sut.CreateAsync(ValidCreateRequest("dup@test.local"));

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("kullanılıyor");
    }

    // 7. Zayıf parola reddi
    [Fact]
    public async Task CreateAsync_WithWeakPassword_IsRejected()
    {
        var result = await _sut.CreateAsync(new CreateUserRequest
        {
            Email = "weak@test.local",
            Password = "123",
            Role = ApplicationRoles.ContentEditor
        });

        result.Succeeded.Should().BeFalse();
    }

    // 8. Rol seçilmemesi reddi
    [Fact]
    public async Task CreateAsync_WithNoRole_IsRejected()
    {
        var result = await _sut.CreateAsync(new CreateUserRequest
        {
            Email = "norole@test.local",
            Password = ValidPassword,
            Role = string.Empty
        });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Rol");
    }

    // 9. Geçersiz rol reddi
    [Fact]
    public async Task CreateAsync_WithInvalidRole_IsRejected()
    {
        var result = await _sut.CreateAsync(new CreateUserRequest
        {
            Email = "badrole@test.local",
            Password = ValidPassword,
            Role = "Süper Admin"
        });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Geçersiz rol");
    }

    // 10. Tek rol atama
    [Fact]
    public async Task CreateAsync_WithSingleRole_Succeeds()
    {
        var dto = await CreateAndGetAsync("single@test.local", ApplicationRoles.SeoEditor);

        dto.Role.Should().Be(ApplicationRoles.SeoEditor);
    }

    // 11. Kullanıcının AspNetUserRoles'ta tam olarak bir kaydı olur (çoklu rol desteği yok)
    [Fact]
    public async Task CreateAsync_AssignsExactlyOneRole()
    {
        var created = await CreateAndGetAsync("exactlyone@test.local", ApplicationRoles.ProductManager);

        var user = await _userManager.FindByIdAsync(created.Id);
        var roles = await _userManager.GetRolesAsync(user!);
        roles.Should().ContainSingle().Which.Should().Be(ApplicationRoles.ProductManager);
    }

    // 12. UpdateAsync Email/UserName'i değiştirmez (Task 16B revizyon kararı)
    [Fact]
    public async Task UpdateAsync_DoesNotChangeEmailOrUserName()
    {
        var created = await CreateAndGetAsync("immutable-email@test.local");

        var result = await _sut.UpdateAsync(created.Id, new UpdateUserRequest
        {
            Role = ApplicationRoles.SeoEditor,
            IsActive = true
        }, currentUserId: "someone-else");

        result.Succeeded.Should().BeTrue();
        var updated = await _sut.GetByIdAsync(created.Id);
        updated!.Email.Should().Be("immutable-email@test.local");
        var user = await _userManager.FindByIdAsync(created.Id);
        user!.UserName.Should().Be("immutable-email@test.local");
    }

    // 13. Rol güncelleme
    [Fact]
    public async Task UpdateAsync_ChangesRole()
    {
        var created = await CreateAndGetAsync("roleupdate@test.local", ApplicationRoles.ContentEditor);

        var result = await _sut.UpdateAsync(created.Id, new UpdateUserRequest
        {
            Role = ApplicationRoles.ProductManager,
            IsActive = true
        }, currentUserId: "someone-else");

        result.Succeeded.Should().BeTrue();
        var updated = await _sut.GetByIdAsync(created.Id);
        updated!.Role.Should().Be(ApplicationRoles.ProductManager);
    }

    // 14. Rol değişince eski rol AspNetUserRoles'tan kaldırılır (tek-rol invaryantı korunur)
    [Fact]
    public async Task UpdateAsync_RemovesOldRoleWhenRoleChanges()
    {
        var created = await CreateAndGetAsync("swap@test.local", ApplicationRoles.ContentEditor);

        var result = await _sut.UpdateAsync(created.Id, new UpdateUserRequest
        {
            Role = ApplicationRoles.SeoEditor,
            IsActive = true
        }, currentUserId: "someone-else");

        result.Succeeded.Should().BeTrue();
        var user = await _userManager.FindByIdAsync(created.Id);
        (await _userManager.IsInRoleAsync(user!, ApplicationRoles.ContentEditor)).Should().BeFalse();
        var roles = await _userManager.GetRolesAsync(user!);
        roles.Should().ContainSingle().Which.Should().Be(ApplicationRoles.SeoEditor);
    }

    // 15. Aktifleştirme
    [Fact]
    public async Task ActivateAsync_SetsIsActiveTrue()
    {
        var created = await CreateAndGetAsync("toactivate@test.local");
        await _sut.DeactivateAsync(created.Id, currentUserId: "someone-else");

        var result = await _sut.ActivateAsync(created.Id);

        result.Succeeded.Should().BeTrue();
        (await _sut.GetByIdAsync(created.Id))!.IsActive.Should().BeTrue();
    }

    // 16. Pasifleştirme
    [Fact]
    public async Task DeactivateAsync_SetsIsActiveFalse()
    {
        var created = await CreateAndGetAsync("todeactivate@test.local");

        var result = await _sut.DeactivateAsync(created.Id, currentUserId: "someone-else");

        result.Succeeded.Should().BeTrue();
        (await _sut.GetByIdAsync(created.Id))!.IsActive.Should().BeFalse();
    }

    // 17. Kendi hesabını pasifleştirme reddi
    [Fact]
    public async Task DeactivateAsync_OwnAccount_IsRejected()
    {
        var created = await CreateAndGetAsync("self-deactivate@test.local");

        var result = await _sut.DeactivateAsync(created.Id, currentUserId: created.Id);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Kendi hesabınızı");
        (await _sut.GetByIdAsync(created.Id))!.IsActive.Should().BeTrue();
    }

    // 18. Son aktif Admin'i pasifleştirme reddi
    [Fact]
    public async Task DeactivateAsync_LastActiveAdmin_IsRejected()
    {
        var admin = await CreateAndGetAsync("lone-admin@test.local", ApplicationRoles.Admin);

        var result = await _sut.DeactivateAsync(admin.Id, currentUserId: "someone-else");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("son aktif Admin");
        (await _sut.GetByIdAsync(admin.Id))!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateAsync_AdminWithOtherActiveAdminPresent_Succeeds()
    {
        var admin1 = await CreateAndGetAsync("admin1@test.local", ApplicationRoles.Admin);
        await CreateAndGetAsync("admin2@test.local", ApplicationRoles.Admin);

        var result = await _sut.DeactivateAsync(admin1.Id, currentUserId: "someone-else");

        result.Succeeded.Should().BeTrue();
    }

    // 19. Parola sıfırlama
    [Fact]
    public async Task ResetPasswordAsync_ChangesPassword()
    {
        var created = await CreateAndGetAsync("resetpw@test.local");

        var result = await _sut.ResetPasswordAsync(created.Id, new ResetUserPasswordRequest { NewPassword = "New-P@ssw0rd-456" });

        result.Succeeded.Should().BeTrue();
        var user = await _userManager.FindByIdAsync(created.Id);
        (await _userManager.CheckPasswordAsync(user!, "New-P@ssw0rd-456")).Should().BeTrue();
    }

    // 20. Parola sıfırlama sonrası eski parola çalışmıyor
    [Fact]
    public async Task ResetPasswordAsync_OldPasswordNoLongerWorks()
    {
        var created = await CreateAndGetAsync("resetpwold@test.local");

        await _sut.ResetPasswordAsync(created.Id, new ResetUserPasswordRequest { NewPassword = "New-P@ssw0rd-456" });

        var user = await _userManager.FindByIdAsync(created.Id);
        (await _userManager.CheckPasswordAsync(user!, ValidPassword)).Should().BeFalse();
    }

    // 21. Hard-delete
    [Fact]
    public async Task DeleteAsync_RemovesUser()
    {
        var created = await CreateAndGetAsync("todelete@test.local");

        var result = await _sut.DeleteAsync(created.Id, currentUserId: "someone-else");

        result.Succeeded.Should().BeTrue();
        (await _sut.GetByIdAsync(created.Id)).Should().BeNull();
    }

    // 22. Kendi hesabını silme reddi
    [Fact]
    public async Task DeleteAsync_OwnAccount_IsRejected()
    {
        var created = await CreateAndGetAsync("self-delete@test.local");

        var result = await _sut.DeleteAsync(created.Id, currentUserId: created.Id);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Kendi hesabınızı");
        (await _sut.GetByIdAsync(created.Id)).Should().NotBeNull();
    }

    // 23. Son aktif Admin'i silme reddi
    [Fact]
    public async Task DeleteAsync_LastActiveAdmin_IsRejected()
    {
        var admin = await CreateAndGetAsync("lone-admin-delete@test.local", ApplicationRoles.Admin);

        var result = await _sut.DeleteAsync(admin.Id, currentUserId: "someone-else");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("son aktif Admin");
        (await _sut.GetByIdAsync(admin.Id)).Should().NotBeNull();
    }

    // 24. Kendi Admin rolünü kaldırma reddi
    [Fact]
    public async Task UpdateAsync_RemovingOwnAdminRole_IsRejected()
    {
        var admin1 = await CreateAndGetAsync("self-admin1@test.local", ApplicationRoles.Admin);
        await CreateAndGetAsync("self-admin2@test.local", ApplicationRoles.Admin); // başka aktif admin var, last-admin guardrail'i tetiklenmesin

        var result = await _sut.UpdateAsync(admin1.Id, new UpdateUserRequest
        {
            Role = ApplicationRoles.ContentEditor,
            IsActive = true
        }, currentUserId: admin1.Id);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Kendi Admin rolünüzü");
        (await _sut.GetByIdAsync(admin1.Id))!.Role.Should().Be(ApplicationRoles.Admin);
    }

    // 25. Son aktif Admin'in Admin rolünü kaldırma reddi
    [Fact]
    public async Task UpdateAsync_RemovingLastActiveAdminRole_IsRejected()
    {
        var admin = await CreateAndGetAsync("lone-admin-role@test.local", ApplicationRoles.Admin);

        var result = await _sut.UpdateAsync(admin.Id, new UpdateUserRequest
        {
            Role = ApplicationRoles.ContentEditor,
            IsActive = true
        }, currentUserId: "someone-else");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("son aktif Admin");
        (await _sut.GetByIdAsync(admin.Id))!.Role.Should().Be(ApplicationRoles.Admin);
    }

    // 26. Var olmayan kullanıcı davranışları
    [Fact]
    public async Task GetByIdAsync_NonExistentUser_ReturnsNull()
    {
        (await _sut.GetByIdAsync("does-not-exist")).Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_NonExistentUser_ReturnsFailure()
    {
        var result = await _sut.UpdateAsync("does-not-exist", new UpdateUserRequest
        {
            Role = ApplicationRoles.Admin,
            IsActive = true
        }, currentUserId: "someone-else");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task DeleteAsync_NonExistentUser_ReturnsFailure()
    {
        var result = await _sut.DeleteAsync("does-not-exist", currentUserId: "someone-else");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task ActivateAsync_NonExistentUser_ReturnsFailure()
    {
        var result = await _sut.ActivateAsync("does-not-exist");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task DeactivateAsync_NonExistentUser_ReturnsFailure()
    {
        var result = await _sut.DeactivateAsync("does-not-exist", currentUserId: "someone-else");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task ResetPasswordAsync_NonExistentUser_ReturnsFailure()
    {
        var result = await _sut.ResetPasswordAsync("does-not-exist", new ResetUserPasswordRequest { NewPassword = ValidPassword });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("bulunamadı");
    }
}
