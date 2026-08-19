using Application;
using Application.Categories;
using Application.Collections;
using Application.Banners;
using Application.Dealers;
using Application.Forms;
using Application.Languages;
using Application.ProductImport;
using Application.Users;
using Application.Blogs;
using Application.Documents;
using Application.News;
using Application.Pages;
using Application.ProductImages;
using Application.Dashboard;
using Application.Products;
using Application.ReferenceProjects;
using Application.Roles;
using Application.Storage;
using Application.Translations;
using Application.Surfaces;
using Infrastructure.Identity;
using Infrastructure.Email;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.ProductImport;
using Infrastructure.ProductImageImport;
using Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });
        services.AddMemoryCache();

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                if (configuration.GetValue<bool>("Identity:AllowWeakPasswords"))
                {
                    options.Password.RequiredLength = 1;
                    options.Password.RequiredUniqueChars = 1;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                }
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITranslationService, TranslationService>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICollectionRepository, CollectionRepository>();
        services.AddScoped<ISurfaceRepository, SurfaceRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductImageRepository, ProductImageRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IReferenceProjectRepository, ReferenceProjectRepository>();
        services.AddScoped<IReferenceProjectImageRepository, ReferenceProjectImageRepository>();
        services.AddScoped<IBlogRepository, BlogRepository>();
        services.AddScoped<IBlogCategoryRepository, BlogCategoryRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<INewsRepository, NewsRepository>();
        services.AddScoped<INewsCategoryRepository, NewsCategoryRepository>();
        services.AddScoped<IBannerRepository, BannerRepository>();
        services.AddScoped<IDealerRepository, DealerRepository>();
        services.AddScoped<IDealerImageRepository, DealerImageRepository>();
        services.AddScoped<IFormSubmissionRepository, FormSubmissionRepository>();
        services.AddScoped<INotificationSettingsRepository, NotificationSettingsRepository>();
        services.AddScoped<IEmailNotificationService, SmtpEmailNotificationService>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IPageRepository, PageRepository>();
        services.AddScoped<IPageContentBlockRepository, PageContentBlockRepository>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IProductImportFileReader, ExcelProductImportFileReader>();
        services.AddScoped<IProductImportTemplateWriter, ExcelProductImportTemplateWriter>();
        services.AddScoped<IProductCatalogResetService, ProductCatalogResetService>();
        services.AddScoped<ProductImageImportCliService>();

        return services;
    }
}
