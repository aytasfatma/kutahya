using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NGKutahyaSeramik.IntegrationTests.Authentication;
using NGKutahyaSeramik.UnitTests.Common;

namespace NGKutahyaSeramik.IntegrationTests.Fixtures;

/// <summary>
/// Program.cs'i (gerçek uygulama pipeline'ı — RBAC/AntiForgery/PRG/routing/middleware sırası)
/// olduğu gibi çalıştırır; yalnızca iki şey testler için değiştirilir:
/// 1) SQL Server yerine SQLite in-memory DbContext (EF Core ilişkileri/constraint'leri gerçekten
///    sınamak için — UseInMemoryDatabase KULLANILMAZ, bkz. SqliteTestDatabase).
/// 2) Cookie/Identity authentication yerine TestAuthHandler (X-Test-Role header'ından rol claim'i).
/// AntiForgery, routing, [Authorize], PRG, TempData middleware'lerinin hiçbiri değiştirilmez.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // AddInfrastructureServices, konfigürasyonda "ConnectionStrings:DefaultConnection" bulunmazsa
        // (Testing ortamı için appsettings.Testing.json yok) fırlar — gerçekte hiç kullanılmayacak
        // (aşağıda DbContext SQLite ile değiştiriliyor) ama Program.cs'in ilk okuduğu an dolu olmalı.
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", "Data Source=unused;")
            ]);
        });

        builder.ConfigureServices(services =>
        {
            // Yalnızca DbContextOptions<AppDbContext>'i kaldırmak yetmiyor — EF Core, AddDbContext'in
            // SqlServer çağrısından kalan IDbContextOptionsConfiguration<AppDbContext> girdilerini de
            // (çoklu-configure birleştirme mekanizması) ayrıca temizlemek gerekiyor; aksi halde "hem
            // SqlServer hem Sqlite sağlayıcısı kayıtlı" hatası oluşuyor.
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Program.cs'in kendi seed bloğu (SeedIdentityDataAsync/SeedLanguagesAsync), builder.Build()
            // sonrasında çalışır — o çalışmadan ÖNCE şemanın var olması gerekir, bu yüzden burada
            // (ConfigureServices, henüz servis sağlayıcısı kurulmamışken) düz bir DbContext ile
            // EnsureCreated çağrılır. Aynı açık bağlantıyı paylaştığı için DI'dan üretilecek sonraki
            // AppDbContext örnekleri de aynı şemayı/veriyi görür.
            var bootstrapOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .ReplaceService<IModelCustomizer, SqliteCompatibleModelCustomizer>()
                .Options;
            using (var bootstrapContext = new AppDbContext(bootstrapOptions))
            {
                bootstrapContext.Database.EnsureCreated();
            }

            services.AddDbContext<AppDbContext>(options => options
                .UseSqlite(_connection)
                .ReplaceService<IModelCustomizer, SqliteCompatibleModelCustomizer>());

            // WebApplicationFactory.Dispose() bu sürümde virtual değil (override edilemiyor) — açık
            // bağlantının host ile birlikte kapatılmasını garanti etmek için container'a singleton
            // olarak kaydedilir; ServiceProvider dispose edildiğinde IDisposable singleton'lar
            // otomatik olarak Dispose edilir (ASP.NET Core DI'nin standart davranışı).
            services.AddSingleton(_connection);

            // Cookie/Identity authentication şemasını test şemasıyla değiştir (RBAC middleware'i,
            // AntiForgery, routing hiçbiri değişmez — yalnızca "kim giriş yapmış" sorusu).
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.PostConfigure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
            {
                options.DefaultScheme = TestAuthHandler.SchemeName;
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultForbidScheme = TestAuthHandler.SchemeName;
            });
        });
    }
}
