using Application.Products;
using Application.Categories;
using Application.Collections;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private const string TrLanguageCode = "TR";
    private const string TechnicalOptionsCacheKey = "products:technical-options";
    private static readonly TimeSpan TechnicalOptionsCacheDuration = TimeSpan.FromMinutes(2);

    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache? _memoryCache;

    public ProductRepository(AppDbContext dbContext, IMemoryCache? memoryCache = null)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public Task<Product?> GetByIdAsync(int id) =>
        _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);

    public Task<Product?> GetBySeoUrlAsync(string seoUrl, string languageCode)
    {
        var normalizedUrl = seoUrl.Trim();
        var normalizedLanguage = languageCode.Trim().ToUpperInvariant();
        return _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(product =>
            _dbContext.Translations.Any(translation =>
                translation.EntityType == EntityType.Product &&
                translation.EntityId == product.Id &&
                translation.Language.Code == normalizedLanguage &&
                translation.FieldName == ProductFields.SeoUrl &&
                translation.Value == normalizedUrl));
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync() =>
        await _dbContext.Products.AsNoTracking().ToListAsync();

    public async Task<(IReadOnlyList<Product> Items, int TotalCount, int PageNumber)> GetPagedAsync(ProductQuery query)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 25 : query.PageSize;

        var filtered = _dbContext.Products.AsNoTracking();
        if (query.Status.HasValue) filtered = filtered.Where(p => p.Status == query.Status.Value);
        if (query.CategoryId.HasValue) filtered = filtered.Where(p => p.CategoryId == query.CategoryId.Value);
        if (query.CollectionId.HasValue) filtered = filtered.Where(p => p.CollectionId == query.CollectionId.Value);
        if (query.Brand.HasValue)
        {
            var brand = query.Brand.Value;
            var brandName = brand.ToString();
            filtered = filtered.Where(p => p.Brand == brand || p.BrandValues.Contains(brandName));
        }
        if (!string.IsNullOrWhiteSpace(query.Surface)) filtered = filtered.Where(p => p.Surface == query.Surface);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            filtered = filtered.Where(p => p.ProductCode.Contains(search) || _dbContext.Translations.Any(t =>
                t.EntityType == EntityType.Product && t.EntityId == p.Id && t.Language.Code == TrLanguageCode &&
                t.FieldName == ProductFields.Name && t.Value.Contains(search)));
        }
        var totalCount = await filtered.CountAsync();
        var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages > 0 && pageNumber > totalPages)
        {
            pageNumber = totalPages;
        }

        var ordered = query.Sort switch
        {
            ProductSortOptions.ProductCode => filtered.OrderBy(p => p.ProductCode).ThenBy(p => p.DisplayOrder),
            ProductSortOptions.Newest => filtered.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.ProductCode),
            ProductSortOptions.NameAsc => filtered.OrderBy(p => _dbContext.Translations
                    .Where(t => t.EntityType == EntityType.Product &&
                        t.EntityId == p.Id &&
                        t.Language.Code == TrLanguageCode &&
                        t.FieldName == ProductFields.Name)
                    .Select(t => t.Value)
                    .FirstOrDefault())
                .ThenBy(p => p.ProductCode),
            _ => filtered.OrderBy(p => p.DisplayOrder).ThenBy(p => p.ProductCode)
        };

        var items = await ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount, pageNumber);
    }

    public async Task<ProductPagedResult<ProductListItemDto>> GetPagedListAsync(ProductQuery query)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 25 : query.PageSize;

        var filtered = _dbContext.Products.AsNoTracking();
        if (query.Status.HasValue) filtered = filtered.Where(p => p.Status == query.Status.Value);
        if (query.CategoryId.HasValue) filtered = filtered.Where(p => p.CategoryId == query.CategoryId.Value);
        if (query.CollectionId.HasValue) filtered = filtered.Where(p => p.CollectionId == query.CollectionId.Value);
        if (query.Brand.HasValue)
        {
            var brand = query.Brand.Value;
            var brandName = brand.ToString();
            filtered = filtered.Where(p => p.Brand == brand || p.BrandValues.Contains(brandName));
        }
        if (!string.IsNullOrWhiteSpace(query.Surface)) filtered = filtered.Where(p => p.Surface == query.Surface);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            filtered = filtered.Where(p => p.ProductCode.Contains(search) || _dbContext.Translations.Any(t =>
                t.EntityType == EntityType.Product && t.EntityId == p.Id && t.Language.Code == TrLanguageCode &&
                t.FieldName == ProductFields.Name && t.Value.Contains(search)));
        }
        var totalCount = await filtered.CountAsync();
        var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages > 0 && pageNumber > totalPages)
        {
            pageNumber = totalPages;
        }

        var ordered = query.Sort switch
        {
            ProductSortOptions.ProductCode => filtered.OrderBy(p => p.ProductCode).ThenBy(p => p.DisplayOrder),
            ProductSortOptions.Newest => filtered.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.ProductCode),
            ProductSortOptions.NameAsc => filtered.OrderBy(p => _dbContext.Translations
                    .Where(t => t.EntityType == EntityType.Product &&
                        t.EntityId == p.Id &&
                        t.Language.Code == TrLanguageCode &&
                        t.FieldName == ProductFields.Name)
                    .Select(t => t.Value)
                    .FirstOrDefault())
                .ThenBy(p => p.ProductCode),
            _ => filtered.OrderBy(p => p.DisplayOrder).ThenBy(p => p.ProductCode)
        };

        var items = await ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                DisplayName = _dbContext.Translations
                    .Where(t => t.EntityType == EntityType.Product &&
                        t.EntityId == p.Id &&
                        t.Language.Code == TrLanguageCode &&
                        t.FieldName == ProductFields.Name)
                    .Select(t => t.Value)
                    .FirstOrDefault(),
                SeoUrl = _dbContext.Translations
                    .Where(t => t.EntityType == EntityType.Product && t.EntityId == p.Id && t.Language.Code == TrLanguageCode && t.FieldName == ProductFields.SeoUrl)
                    .Select(t => t.Value).FirstOrDefault(),
                ShortDescription = _dbContext.Translations
                    .Where(t => t.EntityType == EntityType.Product && t.EntityId == p.Id && t.Language.Code == TrLanguageCode && t.FieldName == ProductFields.ShortDescription)
                    .Select(t => t.Value).FirstOrDefault(),
                CategoryId = p.CategoryId,
                CategoryName = _dbContext.Translations
                    .Where(t => t.EntityType == EntityType.Category &&
                        t.EntityId == p.CategoryId &&
                        t.Language.Code == TrLanguageCode &&
                        t.FieldName == CategoryFields.Name)
                    .Select(t => t.Value)
                    .FirstOrDefault(),
                CollectionName = _dbContext.Translations
                    .Where(t => t.EntityType == EntityType.Collection &&
                        t.EntityId == p.CollectionId &&
                        t.Language.Code == TrLanguageCode &&
                        t.FieldName == CollectionFields.Name)
                    .Select(t => t.Value)
                    .FirstOrDefault(),
                CollectionId = p.CollectionId,
                PrimaryImagePath = _dbContext.ProductImages
                    .Where(i => i.ProductId == p.Id)
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.DisplayOrder)
                    .ThenBy(i => i.Id)
                    .Select(i => i.FilePath)
                    .FirstOrDefault(),
                Brand = p.Brand,
                BrandValues = p.BrandValues,
                Status = p.Status,
                Size = p.Size,
                Surface = p.Surface,
                Color = p.Color,
                DisplayOrder = p.DisplayOrder
            })
            .ToListAsync();

        return new ProductPagedResult<ProductListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ProductTechnicalOptionsDto> GetTechnicalOptionsAsync()
    {
        if (_memoryCache is not null &&
            _memoryCache.TryGetValue(TechnicalOptionsCacheKey, out ProductTechnicalOptionsDto? cachedOptions) &&
            cachedOptions is not null)
        {
            return cachedOptions;
        }

        var options = new ProductTechnicalOptionsDto
        {
            Sizes = await GetDistinctStringOptionsAsync(_dbContext.Products.AsNoTracking().Select(p => p.Size)),
            Units = await GetDistinctStringOptionsAsync(_dbContext.Products.AsNoTracking().Select(p => p.Unit)),
            Surfaces = await GetDistinctStringOptionsAsync(_dbContext.Products.AsNoTracking().Select(p => p.Surface)),
            Reliefs = await GetDistinctStringOptionsAsync(_dbContext.Products.AsNoTracking().Select(p => p.Relief)),
            SpecialSurfaces = await GetDistinctStringOptionsAsync(_dbContext.Products.AsNoTracking().Select(p => p.SpecialSurface)),
            BodyTypes = await GetDistinctStringOptionsAsync(_dbContext.Products.AsNoTracking().Select(p => p.BodyType)),
            Colors = await GetDistinctStringOptionsAsync(_dbContext.Products.AsNoTracking().Select(p => p.Color)),
            ColorMaterials = await GetDistinctStringOptionsAsync(_dbContext.Products.AsNoTracking().Select(p => p.ColorMaterial)),
            ApplicationAreas = BuildMultiTextOptions(await GetDistinctStringOptionsAsync(_dbContext.Products.AsNoTracking().Select(p => p.ApplicationArea))),
            UsageAreas = BuildMultiTextOptions(await GetDistinctStringOptionsAsync(_dbContext.Products.AsNoTracking().Select(p => p.UsageArea))),
            Finishes = await GetDistinctStringOptionsAsync(_dbContext.Products.AsNoTracking().Select(p => p.Finish)),
            DeepAbrasions = BuildDeepAbrasionOptions(await GetDistinctStringOptionsAsync(_dbContext.Products.AsNoTracking().Select(p => p.DeepAbrasion))),
            BoxM2Values = await GetDistinctDecimalOptionsAsync(_dbContext.Products.AsNoTracking().Select(p => p.BoxM2)),
            PalletM2Values = await GetDistinctDecimalOptionsAsync(_dbContext.Products.AsNoTracking().Select(p => p.PalletM2))
        };

        _memoryCache?.Set(TechnicalOptionsCacheKey, options, TechnicalOptionsCacheDuration);

        return options;
    }

    public async Task<int> GetNextDisplayOrderAsync()
    {
        var maxDisplayOrder = await _dbContext.Products
            .AsNoTracking()
            .MaxAsync(p => (int?)p.DisplayOrder);

        return (maxDisplayOrder ?? 0) + 1;
    }

    public async Task<IReadOnlyList<(int Id, string Label)>> GetProductCodeOptionsAsync()
    {
        return await _dbContext.Products
            .AsNoTracking()
            .OrderBy(p => p.ProductCode)
            .ThenBy(p => p.Id)
            .Select(p => new ValueTuple<int, string>(p.Id, p.ProductCode))
            .ToListAsync();
    }

    public Task<Product?> GetByProductCodeAsync(string productCode) =>
        _dbContext.Products.FirstOrDefaultAsync(p => p.ProductCode == productCode);

    public Task<bool> HasAnyWithCategoryIdAsync(int categoryId) =>
        _dbContext.Products.AsNoTracking().AnyAsync(p => p.CategoryId == categoryId);

    public Task<int> CountByCategoryIdAsync(int categoryId) =>
        _dbContext.Products.AsNoTracking().CountAsync(p => p.CategoryId == categoryId);

    public Task<bool> HasAnyWithCollectionIdAsync(int collectionId) =>
        _dbContext.Products.AsNoTracking().AnyAsync(p => p.CollectionId == collectionId);

    public async Task AddAsync(Product product) =>
        await _dbContext.Products.AddAsync(product);

    public void Remove(Product product) =>
        _dbContext.Products.Remove(product);

    private static async Task<IReadOnlyList<string>> GetDistinctStringOptionsAsync(IQueryable<string?> values)
    {
        var distinctValues = await values
            .Where(value => value != null && value != "" && value != "-")
            .Select(value => value!.Trim())
            .Where(value => value != "" && value != "-")
            .Distinct()
            .ToListAsync();

        return distinctValues
            .OrderBy(value => value, StringComparer.Create(
                new System.Globalization.CultureInfo("tr-TR"),
                ignoreCase: true))
            .ToList();
    }

    private static async Task<IReadOnlyList<decimal>> GetDistinctDecimalOptionsAsync(IQueryable<decimal?> values)
    {
        // SQLite (integration testlerinde kullanılıyor) decimal üzerinde ORDER BY'ı SQL'e çeviremiyor
        // (SQL Server'da sorun yok) — sıralama bu yüzden materialize edildikten sonra bellekte yapılır.
        var distinctValues = await values
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .Distinct()
            .ToListAsync();

        return distinctValues.OrderBy(value => value).ToList();
    }

    private static IReadOnlyList<string> BuildMultiTextOptions(IEnumerable<string> values) =>
        values
            .SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(value => !string.IsNullOrWhiteSpace(value) && value != "-")
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(value => value)
            .ToList();

    private static IReadOnlyList<string> BuildDeepAbrasionOptions(IEnumerable<string> values)
    {
        var options = values
            .Where(value => !value.All(char.IsDigit))
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(value => value)
            .ToList();

        if (!options.Contains("≤ 139 mm³", StringComparer.CurrentCultureIgnoreCase))
        {
            options.Insert(0, "≤ 139 mm³");
        }

        return options;
    }
}
