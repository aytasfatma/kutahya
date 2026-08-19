using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NGKutahyaSeramik.UnitTests.Common;

namespace NGKutahyaSeramik.UnitTests.Seeding;

public class LanguageSeederTests
{
    [Fact]
    public async Task SeedLanguagesAsync_FirstRun_Creates7LanguagesInOrder()
    {
        using var host = IdentityTestHost.Create();
        using var scope = host.Services.CreateScope();

        await scope.ServiceProvider.SeedLanguagesAsync();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var codes = await db.Languages.OrderBy(l => l.DisplayOrder).Select(l => l.Code).ToListAsync();

        codes.Should().Equal("TR", "EN", "DE", "FR", "ES", "AR", "RU");
    }

    [Fact]
    public async Task SeedLanguagesAsync_SecondRun_DoesNotCreateDuplicates()
    {
        using var host = IdentityTestHost.Create();

        using (var scope1 = host.Services.CreateScope())
        {
            await scope1.ServiceProvider.SeedLanguagesAsync();
        }

        using var scope2 = host.Services.CreateScope();
        await scope2.ServiceProvider.SeedLanguagesAsync();

        var db = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Languages.CountAsync()).Should().Be(7);
    }

    [Fact]
    public async Task SeedLanguagesAsync_WithPreExistingLanguage_PreservesItAndAddsMissingOnes()
    {
        using var host = IdentityTestHost.Create();

        using (var scope1 = host.Services.CreateScope())
        {
            var db1 = scope1.ServiceProvider.GetRequiredService<AppDbContext>();
            db1.Languages.Add(new Domain.Entities.Language("TR", "Türkçe (Özel)", isActive: true, displayOrder: 1));
            await db1.SaveChangesAsync();
        }

        using var scope2 = host.Services.CreateScope();
        await scope2.ServiceProvider.SeedLanguagesAsync();

        var db2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db2.Languages.CountAsync()).Should().Be(7);
        var tr = await db2.Languages.SingleAsync(l => l.Code == "TR");
        tr.Name.Should().Be("Türkçe (Özel)");
    }
}
