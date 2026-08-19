using Application.Banners;
using Application.Dealers;
using Application.Forms;
using Application.Blogs;
using Application.Categories;
using Application.Collections;
using Application.Documents;
using Application.Languages;
using Application.News;
using Application.Pages;
using Application.ProductImages;
using Application.ProductImport;
using Application.Products;
using Application.ReferenceProjects;
using Application.Seo;
using Application.Translations;
using Application.Surfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<CategoryService>();
        services.AddScoped<CollectionService>();
        services.AddScoped<SurfaceService>();
        services.AddScoped<ProductService>();
        services.AddScoped<ProductImageService>();
        services.AddScoped<DocumentService>();
        services.AddScoped<ReferenceProjectImageService>();
        services.AddScoped<ReferenceProjectService>();
        services.AddScoped<BlogCategoryService>();
        services.AddScoped<BlogService>();
        services.AddScoped<NewsCategoryService>();
        services.AddScoped<NewsService>();
        services.AddScoped<BannerService>();
        services.AddScoped<DealerService>();
        services.AddScoped<DealerImageService>();
        services.AddScoped<FormSubmissionService>();
        services.AddScoped<NotificationSettingsService>();
        services.AddScoped<PageContentBlockService>();
        services.AddScoped<PageService>();
        services.AddScoped<LanguageService>();
        services.AddScoped<SeoManagementService>();
        services.AddScoped<TranslationCoverageService>();
        services.AddScoped<ProductImportService>();
        services.AddScoped<ProductImportCliService>();

        return services;
    }
}
