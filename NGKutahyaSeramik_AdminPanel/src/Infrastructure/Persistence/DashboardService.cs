using Application.Dashboard;
using Application.Banners;
using Application.Blogs;
using Application.Categories;
using Application.Collections;
using Application.News;
using Application.Pages;
using Application.Products;
using Application.ReferenceProjects;
using Application.Translations;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Persistence;

/// <summary>
/// Madde 17.2 modül #1 (Dashboard, Task 18) — salt-okunur özet. `AppDbContext`'e doğrudan bağımlı
/// (ADR-016'nın genel ilkesinin genişletilmiş uygulanışı, bkz. IDashboardService xmldoc) — mevcut
/// 6 modül repository'sinden hiçbirine dokunulmadı. Tüm sorgular `AsNoTracking()` (salt-okunur),
/// sayaçlar DB seviyesinde `CountAsync()`/`Where().CountAsync()`, son kayıtlar
/// `OrderByDescending(CreatedAt).Take(5)` ile — hiçbir tam tablo belleğe çekilmiyor.
/// MissingTranslationCount istisna: Application katmanındaki `TranslationCoverageService`'in
/// tespit mantığını burada tekrar etmek yerine doğrudan onu çağırır (Infrastructure, Application'a
/// zaten bağımlı — tek doğruluk kaynağı korunur).
/// </summary>
public class DashboardService : IDashboardService
{
    private const int RecentItemsLimit = 5;
    private const string MissingTranslationCountCacheKey = "dashboard:missing-translation-count";
    private static readonly TimeSpan MissingTranslationCountCacheDuration = TimeSpan.FromMinutes(1);

    private static readonly IReadOnlyList<(EntityType Type, int FieldCount)> TranslationRequirements =
    [
        (EntityType.Product, 6),
        (EntityType.Category, 5),
        (EntityType.Collection, 5),
        (EntityType.Blog, 6),
        (EntityType.BlogCategory, 1),
        (EntityType.News, 5),
        (EntityType.NewsCategory, 1),
        (EntityType.Page, 4),
        (EntityType.PageContentBlock, 2),
        (EntityType.Banner, 4),
        (EntityType.ReferenceProject, 3)
    ];

    private static readonly IReadOnlyDictionary<EntityType, string[]> RequiredFieldsByType =
        new Dictionary<EntityType, string[]>
        {
            [EntityType.Product] =
            [
                ProductFields.Name,
                ProductFields.ShortDescription,
                ProductFields.LongDescription,
                ProductFields.SeoUrl,
                ProductFields.MetaTitle,
                ProductFields.MetaDescription
            ],
            [EntityType.Category] =
            [
                CategoryFields.Name,
                CategoryFields.Description,
                CategoryFields.SeoUrl,
                CategoryFields.MetaTitle,
                CategoryFields.MetaDescription
            ],
            [EntityType.Collection] =
            [
                CollectionFields.Name,
                CollectionFields.Description,
                CollectionFields.SeoUrl,
                CollectionFields.MetaTitle,
                CollectionFields.MetaDescription
            ],
            [EntityType.Blog] =
            [
                BlogFields.Title,
                BlogFields.Excerpt,
                BlogFields.Content,
                BlogFields.SeoUrl,
                BlogFields.MetaTitle,
                BlogFields.MetaDescription
            ],
            [EntityType.BlogCategory] = [BlogCategoryFields.Name],
            [EntityType.News] =
            [
                NewsFields.Title,
                NewsFields.Content,
                NewsFields.SeoUrl,
                NewsFields.MetaTitle,
                NewsFields.MetaDescription
            ],
            [EntityType.NewsCategory] = [NewsCategoryFields.Name],
            [EntityType.Page] =
            [
                PageFields.Title,
                PageFields.SeoUrl,
                PageFields.MetaTitle,
                PageFields.MetaDescription
            ],
            [EntityType.PageContentBlock] =
            [
                PageContentBlockFields.Title,
                PageContentBlockFields.Content
            ],
            [EntityType.Banner] =
            [
                BannerFields.Title,
                BannerFields.Subtitle,
                BannerFields.ButtonText,
                BannerFields.ButtonUrl
            ],
            [EntityType.ReferenceProject] =
            [
                ReferenceProjectFields.ProjectName,
                ReferenceProjectFields.Description,
                ReferenceProjectFields.SeoUrl
            ]
        };

    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;

    public DashboardService(AppDbContext dbContext, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var totalProducts = await _dbContext.Products.AsNoTracking().CountAsync();
        var activeProducts = await _dbContext.Products.AsNoTracking().CountAsync(p => p.Status == ProductStatus.Active);
        var totalCategories = await _dbContext.Categories.AsNoTracking().CountAsync();
        var totalCollections = await _dbContext.Collections.AsNoTracking().CountAsync();
        var dealerCount = await _dbContext.Dealers.AsNoTracking().CountAsync(d => d.Category == DealerCategory.SalesPoint);
        var showroomCount = await _dbContext.Dealers.AsNoTracking().CountAsync(d => d.Category == DealerCategory.GeneralHeadquarters || d.Category == DealerCategory.Factory);
        var totalUsers = await _dbContext.Users.AsNoTracking().CountAsync();
        var activeUsers = await _dbContext.Users.AsNoTracking().CountAsync(u => u.IsActive);
        var totalFormSubmissions = await _dbContext.FormSubmissions.AsNoTracking().CountAsync();
        var unreadFormSubmissions = await _dbContext.FormSubmissions.AsNoTracking().CountAsync(f => !f.IsRead);
        var unprocessedFormSubmissions = await _dbContext.FormSubmissions.AsNoTracking().CountAsync(f => f.ProcessedAt == null);
        var missingTranslationCount = await GetMissingTranslationCountAsync();

        var recentProducts = await _dbContext.Products
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Take(RecentItemsLimit)
            .Select(p => new DashboardRecentProductDto
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                Status = p.Status,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        var recentForms = await _dbContext.FormSubmissions
            .AsNoTracking()
            .OrderByDescending(f => f.CreatedAt)
            .Take(RecentItemsLimit)
            .Select(f => new DashboardRecentFormDto
            {
                Id = f.Id,
                FormType = f.FormType,
                FullName = f.FullName,
                CreatedAt = f.CreatedAt,
                IsRead = f.IsRead
            })
            .ToListAsync();

        return new DashboardDto
        {
            TotalProducts = totalProducts,
            ActiveProducts = activeProducts,
            TotalCategories = totalCategories,
            TotalCollections = totalCollections,
            DealerCount = dealerCount,
            ShowroomCount = showroomCount,
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            TotalFormSubmissions = totalFormSubmissions,
            UnreadFormSubmissions = unreadFormSubmissions,
            UnprocessedFormSubmissions = unprocessedFormSubmissions,
            MissingTranslationCount = missingTranslationCount,
            RecentProducts = recentProducts,
            RecentForms = recentForms
        };
    }

    private async Task<int> GetMissingTranslationCountAsync()
    {
        if (_memoryCache.TryGetValue(MissingTranslationCountCacheKey, out int cachedCount))
        {
            return cachedCount;
        }

        var activeLanguageIds = await _dbContext.Languages
            .AsNoTracking()
            .Where(l => l.IsActive)
            .Select(l => l.Id)
            .ToListAsync();

        if (activeLanguageIds.Count == 0)
        {
            return 0;
        }

        var activeLanguageCount = activeLanguageIds.Count;
        var requiredCount = 0;

        foreach (var (type, fieldCount) in TranslationRequirements)
        {
            requiredCount += await GetEntityCountAsync(type) * activeLanguageCount * fieldCount;
        }

        var existingCount = 0;
        foreach (var (type, fields) in RequiredFieldsByType)
        {
            existingCount += await _dbContext.Translations
                .AsNoTracking()
                .Where(t => t.EntityType == type &&
                    activeLanguageIds.Contains(t.LanguageId) &&
                    fields.Contains(t.FieldName) &&
                    t.Value.Trim() != string.Empty)
                .CountAsync();
        }

        var missingCount = Math.Max(0, requiredCount - existingCount);
        _memoryCache.Set(MissingTranslationCountCacheKey, missingCount, MissingTranslationCountCacheDuration);

        return missingCount;
    }

    private Task<int> GetEntityCountAsync(EntityType entityType) =>
        entityType switch
        {
            EntityType.Product => _dbContext.Products.AsNoTracking().CountAsync(),
            EntityType.Category => _dbContext.Categories.AsNoTracking().CountAsync(),
            EntityType.Collection => _dbContext.Collections.AsNoTracking().CountAsync(),
            EntityType.Blog => _dbContext.Blogs.AsNoTracking().CountAsync(),
            EntityType.BlogCategory => _dbContext.BlogCategories.AsNoTracking().CountAsync(),
            EntityType.News => _dbContext.News.AsNoTracking().CountAsync(),
            EntityType.NewsCategory => _dbContext.NewsCategories.AsNoTracking().CountAsync(),
            EntityType.Page => _dbContext.Pages.AsNoTracking().CountAsync(),
            EntityType.PageContentBlock => _dbContext.PageContentBlocks.AsNoTracking().CountAsync(),
            EntityType.Banner => _dbContext.Banners.AsNoTracking().CountAsync(),
            EntityType.ReferenceProject => _dbContext.ReferenceProjects.AsNoTracking().CountAsync(),
            _ => Task.FromResult(0)
        };
}
