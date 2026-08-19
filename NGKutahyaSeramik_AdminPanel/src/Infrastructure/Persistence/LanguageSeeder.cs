using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public static class LanguageSeeder
{
    private static readonly (string Code, string Name, int DisplayOrder)[] SeedLanguages =
    [
        ("TR", "Türkçe", 1),
        ("EN", "English", 2),
        ("DE", "Deutsch", 3),
        ("FR", "Français", 4),
        ("ES", "Español", 5),
        ("AR", "العربية", 6),
        ("RU", "Русский", 7)
    ];

    public static async Task SeedLanguagesAsync(this IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(LanguageSeeder));
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

        foreach (var (code, name, displayOrder) in SeedLanguages)
        {
            var exists = await dbContext.Languages.AnyAsync(l => l.Code == code);

            if (exists)
            {
                continue;
            }

            dbContext.Languages.Add(new Language(code, name, isActive: true, displayOrder: displayOrder));
            logger.LogInformation("Dil seed edildi: {Code} ({Name}).", code, name);
        }

        await dbContext.SaveChangesAsync();
    }
}
