using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NGKutahyaSeramik.UnitTests.Common;

/// <summary>
/// IdentitySeeder/LanguageSeeder gibi `IServiceProvider` üzerinden çalışan seeder'ları test etmek
/// için gerçek `AddIdentity`+`AddDbContext(Sqlite)` servis grafiği kurar (RoleManager/UserManager
/// gerçek Identity store'ları gerektirir, salt Moq ile anlamlı test edilemez).
/// </summary>
public sealed class IdentityTestHost : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    public IServiceProvider Services => _provider;

    private IdentityTestHost(SqliteConnection connection, ServiceProvider provider)
    {
        _connection = connection;
        _provider = provider;
    }

    public static IdentityTestHost Create(Dictionary<string, string?>? configurationValues = null)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options => options
            .UseSqlite(connection)
            .ReplaceService<IModelCustomizer, SqliteCompatibleModelCustomizer>());

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues ?? [])
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        }

        return new IdentityTestHost(connection, provider);
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }
}
