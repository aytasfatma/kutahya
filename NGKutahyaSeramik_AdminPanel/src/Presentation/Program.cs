using Application;
using Infrastructure;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Application.ProductImport;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.WriteTo.Console());

builder.Services.AddControllersWithViews();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/data-protection-keys"))
    .SetApplicationName("NGKutahyaSeramikAdminPanel");
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    // SameAsRequest: gerçek isteğin şemasına göre karar verir — HTTPS altında (ör. TLS sonlandıran
    // bir reverse proxy arkasında) davranış Always ile birebir aynı kalır (Secure bayrağı eklenir),
    // ama TLS'siz düz HTTP altında (ör. ADR-006/Dockerfile'ın basitlik için HTTPS kurmadığı Docker
    // Compose demosu, ASPNETCORE_HTTP_PORTS=8080) tarayıcı Secure cookie'yi hiç saklamayacağından
    // Always ile login'in sürekli login'e geri döndüğü bir kilitlenme oluşurdu.
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("public-website", policy => policy
        .WithOrigins(builder.Configuration.GetSection("PublicWebsite:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5500", "http://127.0.0.1:5500"])
        .AllowAnyHeader()
        .AllowAnyMethod());
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("public-forms", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

var app = builder.Build();

// Bu toplu işlem başlangıç seed/migration akışından önce ele alınır. Böylece varsayılan
// product-image-import çalıştırması gerçekten salt-okunur bir dry-run olarak kalır.
if (args.Length >= 2 && string.Equals(args[0], "product-image-import", StringComparison.OrdinalIgnoreCase))
{
    using var imageImportScope = app.Services.CreateScope();
    var cli = imageImportScope.ServiceProvider.GetRequiredService<Infrastructure.ProductImageImport.ProductImageImportCliService>();
    Environment.ExitCode = await cli.RunAsync(args.Skip(1).ToArray());
    return;
}

// ADR-004: migration uygulama yöntemi bilinçli olarak açık bırakılmıştı ("sonraki bir deployment
// kararında ayrıca belirlenecek"). Docker/konteyner dağıtımında etkileşimli CLI olmadığı için
// kontrollü, varsayılan-KAPALI bir opt-in bayrağı eklendi — yerel geliştirme akışı (dotnet ef
// database update, manuel) DEĞİŞMEDİ. appsettings/ortam değişkeni ile açıkça etkinleştirilmediği
// sürece migration burada uygulanmaz.
var applyMigrationsOnStartup = builder.Configuration.GetValue("DatabaseInitialization:ApplyMigrationsOnStartup", defaultValue: false);
var seedOnStartup = builder.Configuration.GetValue("DatabaseInitialization:SeedOnStartup", defaultValue: true);

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        if (applyMigrationsOnStartup)
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        if (seedOnStartup)
        {
            await scope.ServiceProvider.SeedIdentityDataAsync();
            await scope.ServiceProvider.SeedLanguagesAsync();

            if (app.Environment.IsDevelopment())
            {
                await scope.ServiceProvider.SeedDevelopmentTestUserAsync();
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Veritabanı başlatma (migration/seed) işlemi sırasında hata oluştu.");
        throw;
    }
}

if (args.Length >= 2 && string.Equals(args[0], "product-import", StringComparison.OrdinalIgnoreCase))
{
    using var importScope = app.Services.CreateScope();
    var cli = importScope.ServiceProvider.GetRequiredService<ProductImportCliService>();
    var apply = args.Any(x => string.Equals(x, "--apply", StringComparison.OrdinalIgnoreCase));
    Environment.ExitCode = await cli.RunAsync(args[1], apply);
    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("An unexpected error occurred.");
        });
    });
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("public-website");
app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// WebApplicationFactory<Program> (integration testleri) Program sınıfına erişim gerektirir —
// top-level statements varsayılan olarak internal bir Program sınıfı üretir; bu marker olmadan
// test projesi derlenemez. Uygulama davranışını değiştirmez, yalnızca tip görünürlüğünü açar.
public partial class Program
{
}
