using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedIdentityDataAsync(this IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(IdentitySeeder));

        await SeedRolesAsync(serviceProvider);
        await SeedAdminUserAsync(serviceProvider, logger);
    }

    public static async Task SeedDevelopmentTestUserAsync(this IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(IdentitySeeder));

        await SeedTestUserAsync(serviceProvider, logger);
    }

    private static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var roleNames = new[]
        {
            ApplicationRoles.Admin,
            ApplicationRoles.ContentEditor,
            ApplicationRoles.SeoEditor,
            ApplicationRoles.ProductManager
        };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Rol '{roleName}' oluşturulamadı: {string.Join("; ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }

    private static async Task SeedAdminUserAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var adminEmail = configuration["SeedAdmin:Email"];
        var adminPassword = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("SeedAdmin:Email / SeedAdmin:Password tanımlı değil — admin kullanıcı seed'i atlandı.");
            return;
        }

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByEmailAsync(adminEmail);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail
            };

            var createResult = await userManager.CreateAsync(user, adminPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Admin kullanıcı oluşturulamadı: {string.Join("; ", createResult.Errors.Select(e => e.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, ApplicationRoles.Admin))
        {
            var roleResult = await userManager.AddToRoleAsync(user, ApplicationRoles.Admin);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Admin kullanıcı role eklenemedi: {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");
            }
        }
    }

    private static async Task SeedTestUserAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var testUserEmail = configuration["SeedTestUser:Email"];
        var testUserPassword = configuration["SeedTestUser:Password"];

        if (string.IsNullOrWhiteSpace(testUserEmail) || string.IsNullOrWhiteSpace(testUserPassword))
        {
            logger.LogWarning("SeedTestUser:Email / SeedTestUser:Password tanımlı değil — development test kullanıcı seed'i atlandı.");
            return;
        }

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByEmailAsync(testUserEmail);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = testUserEmail,
                Email = testUserEmail
            };

            var createResult = await userManager.CreateAsync(user, testUserPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Development test kullanıcı oluşturulamadı: {string.Join("; ", createResult.Errors.Select(e => e.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, ApplicationRoles.ContentEditor))
        {
            var roleResult = await userManager.AddToRoleAsync(user, ApplicationRoles.ContentEditor);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Development test kullanıcı role eklenemedi: {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");
            }
        }
    }
}
