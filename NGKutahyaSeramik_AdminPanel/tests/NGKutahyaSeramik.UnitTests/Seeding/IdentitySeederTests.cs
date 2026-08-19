using FluentAssertions;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NGKutahyaSeramik.UnitTests.Common;

namespace NGKutahyaSeramik.UnitTests.Seeding;

public class IdentitySeederTests
{
    [Fact]
    public async Task SeedIdentityDataAsync_FirstRun_CreatesAllFourRoles()
    {
        using var host = IdentityTestHost.Create();
        using var scope = host.Services.CreateScope();

        await scope.ServiceProvider.SeedIdentityDataAsync();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleNames = await db.Roles.Select(r => r.Name).ToListAsync();

        roleNames.Should().BeEquivalentTo(
        [
            ApplicationRoles.Admin,
            ApplicationRoles.ContentEditor,
            ApplicationRoles.SeoEditor,
            ApplicationRoles.ProductManager
        ]);
    }

    [Fact]
    public async Task SeedIdentityDataAsync_WithoutAdminConfig_SkipsAdminCreation_DoesNotThrow()
    {
        using var host = IdentityTestHost.Create();
        using var scope = host.Services.CreateScope();

        var act = () => scope.ServiceProvider.SeedIdentityDataAsync();

        await act.Should().NotThrowAsync();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Users.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task SeedIdentityDataAsync_WithAdminConfig_CreatesAdminUserAssignedToAdminRole()
    {
        using var host = IdentityTestHost.Create(new Dictionary<string, string?>
        {
            ["SeedAdmin:Email"] = "admin@test.local",
            ["SeedAdmin:Password"] = "Test-P@ssw0rd-123"
        });
        using var scope = host.Services.CreateScope();

        await scope.ServiceProvider.SeedIdentityDataAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync("admin@test.local");

        admin.Should().NotBeNull();
        (await userManager.IsInRoleAsync(admin!, ApplicationRoles.Admin)).Should().BeTrue();
    }

    [Fact]
    public async Task SeedIdentityDataAsync_SecondRun_DoesNotCreateDuplicateRolesOrUsers()
    {
        using var host = IdentityTestHost.Create(new Dictionary<string, string?>
        {
            ["SeedAdmin:Email"] = "admin@test.local",
            ["SeedAdmin:Password"] = "Test-P@ssw0rd-123"
        });

        using (var scope1 = host.Services.CreateScope())
        {
            await scope1.ServiceProvider.SeedIdentityDataAsync();
        }

        using var scope2 = host.Services.CreateScope();
        await scope2.ServiceProvider.SeedIdentityDataAsync();

        var db = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Roles.CountAsync()).Should().Be(4);
        (await db.Users.CountAsync()).Should().Be(1);
        (await db.UserRoles.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SeedIdentityDataAsync_SecondRun_DoesNotOverwriteExistingPasswordHash()
    {
        using var host = IdentityTestHost.Create(new Dictionary<string, string?>
        {
            ["SeedAdmin:Email"] = "admin@test.local",
            ["SeedAdmin:Password"] = "Test-P@ssw0rd-123"
        });

        string originalHash;
        using (var scope1 = host.Services.CreateScope())
        {
            await scope1.ServiceProvider.SeedIdentityDataAsync();
            var db1 = scope1.ServiceProvider.GetRequiredService<AppDbContext>();
            originalHash = (await db1.Users.SingleAsync()).PasswordHash!;
        }

        using var scope2 = host.Services.CreateScope();
        await scope2.ServiceProvider.SeedIdentityDataAsync();
        var db2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        var hashAfterSecondRun = (await db2.Users.SingleAsync()).PasswordHash!;

        hashAfterSecondRun.Should().Be(originalHash);
    }

    [Fact]
    public async Task SeedIdentityDataAsync_WithPartiallyExistingRoles_CompletesMissingOnes()
    {
        using var host = IdentityTestHost.Create();

        using (var scope1 = host.Services.CreateScope())
        {
            var roleManager = scope1.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Admin));
        }

        using var scope2 = host.Services.CreateScope();
        await scope2.ServiceProvider.SeedIdentityDataAsync();

        var db = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleNames = await db.Roles.Select(r => r.Name).ToListAsync();
        roleNames.Should().BeEquivalentTo(
        [
            ApplicationRoles.Admin,
            ApplicationRoles.ContentEditor,
            ApplicationRoles.SeoEditor,
            ApplicationRoles.ProductManager
        ]);
    }

    [Fact]
    public async Task SeedDevelopmentTestUserAsync_WithConfig_AssignsContentEditorRole()
    {
        using var host = IdentityTestHost.Create(new Dictionary<string, string?>
        {
            ["SeedTestUser:Email"] = "editor@test.local",
            ["SeedTestUser:Password"] = "Test-P@ssw0rd-123"
        });
        using var scope = host.Services.CreateScope();

        // Üretimde Program.cs sırası: SeedIdentityDataAsync (roller) her zaman önce çalışır.
        await scope.ServiceProvider.SeedIdentityDataAsync();
        await scope.ServiceProvider.SeedDevelopmentTestUserAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var editor = await userManager.FindByEmailAsync("editor@test.local");

        editor.Should().NotBeNull();
        (await userManager.IsInRoleAsync(editor!, ApplicationRoles.ContentEditor)).Should().BeTrue();
    }
}
