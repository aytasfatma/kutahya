using Application.Categories;
using Application.Collections;
using Application.Documents;
using Application.Products;
using Application.ProductImages;
using Application.ReferenceProjects;
using Application.Storage;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.Api;

namespace Presentation.Controllers.Api;

[ApiController]
[AllowAnonymous]
[Route("api/public")]
[Produces("application/json")]
public sealed class PublicCatalogController : ControllerBase
{
    private readonly CategoryService _categoryService;
    private readonly CollectionService _collectionService;
    private readonly ProductService _productService;
    private readonly DocumentService _documentService;
    private readonly ReferenceProjectService _referenceProjectService;
    private readonly ReferenceProjectImageService _referenceProjectImageService;
    private readonly IFileStorageService _fileStorageService;

    public PublicCatalogController(
        CategoryService categoryService,
        CollectionService collectionService,
        ProductService productService,
        DocumentService documentService,
        ReferenceProjectService referenceProjectService,
        ReferenceProjectImageService referenceProjectImageService,
        IFileStorageService fileStorageService)
    {
        _categoryService = categoryService;
        _collectionService = collectionService;
        _productService = productService;
        _documentService = documentService;
        _referenceProjectService = referenceProjectService;
        _referenceProjectImageService = referenceProjectImageService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<PublicCategoryResponse>>> Categories(
        [FromQuery] string lang = "tr", [FromQuery] ProductBrand? brand = null)
    {
        var products = (await _productService.GetPagedAsync(new ProductQuery
            {
                PageNumber = 1, PageSize = 2000, Status = ProductStatus.Active, Brand = brand
            })).Items
            .Where(x => x.CategoryId.HasValue)
            .OrderBy(x => x.PrimaryImagePath?.EndsWith("/fallback/deneme.webp", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0)
            .ThenBy(x => x.DisplayOrder)
            .ToList();

        var items = (await _categoryService.GetTreeAsync())
            .Where(x => x.IsActive)
            .Where(x => !brand.HasValue || x.Brands.Contains(brand.Value))
            .Where(x => products.Any(product => product.CategoryId == x.Id))
            .Select(x =>
            {
                var categoryProducts = products.Where(product => product.CategoryId == x.Id).ToList();
                var sampleImage = categoryProducts
                    .FirstOrDefault(product =>
                        !string.IsNullOrWhiteSpace(product.PrimaryImagePath) &&
                        product.PrimaryImagePath?.EndsWith("/fallback/deneme.webp", StringComparison.OrdinalIgnoreCase) != true)
                    ?.PrimaryImagePath;
                sampleImage ??= categoryProducts.FirstOrDefault(product =>
                    !string.IsNullOrWhiteSpace(product.PrimaryImagePath))?.PrimaryImagePath;

                return new PublicCategoryResponse(
                    x.Id, x.ParentCategoryId, x.Name,
                    null, x.SeoUrl, null,
                    null, AssetUrl(sampleImage ?? x.ImagePath), x.DisplayOrder, x.Brands,
                    categoryProducts.Count);
            })
            .OrderBy(x => x.Name, StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), true))
            .ToList();

        return Ok(items);
    }

    [HttpGet("collections")]
    public async Task<ActionResult<IReadOnlyList<PublicCollectionResponse>>> Collections(
        [FromQuery] string lang = "tr", [FromQuery] ProductBrand? brand = null)
    {
        var allProducts = (await _productService.GetPagedAsync(new ProductQuery
            {
                PageNumber = 1, PageSize = 2000, Status = ProductStatus.Active
            })).Items
            .Where(x => !string.IsNullOrWhiteSpace(x.PrimaryImagePath))
            .OrderBy(x => x.PrimaryImagePath?.EndsWith("/fallback/deneme.webp", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0)
            .ThenBy(x => x.DisplayOrder)
            .ToList();
        var products = brand.HasValue
            ? (await _productService.GetPagedAsync(new ProductQuery
                {
                    PageNumber = 1, PageSize = 2000, Status = ProductStatus.Active, Brand = brand
                })).Items.Where(x => !string.IsNullOrWhiteSpace(x.PrimaryImagePath)).ToList()
            : allProducts;
        var items = (await _collectionService.GetAllAsync())
            .Where(x => x.IsActive)
            .Where(x => !brand.HasValue || x.Brands.Contains(brand.Value))
            .Select(x =>
            {
                var sampleImage = products.FirstOrDefault(product =>
                    product.CollectionId == x.Id &&
                    product.PrimaryImagePath?.Contains("/render/", StringComparison.OrdinalIgnoreCase) != true &&
                    product.PrimaryImagePath?.EndsWith("/fallback/deneme.webp", StringComparison.OrdinalIgnoreCase) != true)
                    ?.PrimaryImagePath;
                var sharedCollectionImage = allProducts.FirstOrDefault(product =>
                    product.CollectionId == x.Id &&
                    product.PrimaryImagePath?.Contains("/render/", StringComparison.OrdinalIgnoreCase) != true &&
                    product.PrimaryImagePath?.EndsWith("/fallback/deneme.webp", StringComparison.OrdinalIgnoreCase) != true)
                    ?.PrimaryImagePath;
                var brandSampleImage = products.FirstOrDefault(product =>
                    x.Brands.Contains(product.Brand) &&
                    product.PrimaryImagePath?.Contains("/render/", StringComparison.OrdinalIgnoreCase) != true &&
                    product.PrimaryImagePath?.EndsWith("/fallback/deneme.webp", StringComparison.OrdinalIgnoreCase) != true)
                    ?.PrimaryImagePath;
                return new PublicCollectionResponse(
                    x.Id, x.Name, null,
                    x.SeoUrl, null, null,
                    AssetUrl(sampleImage ?? sharedCollectionImage ?? brandSampleImage ?? products.FirstOrDefault()?.PrimaryImagePath ?? x.ImagePath),
                    x.ProductCount, x.DisplayOrder, x.Brands);
            })
            .OrderBy(x => x.Name, StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), true))
            .ToList();

        return Ok(items);
    }

    [HttpGet("products")]
    public async Task<ActionResult<PublicPagedResponse<PublicProductListResponse>>> Products(
        [FromQuery] string lang = "tr", [FromQuery] int page = 1, [FromQuery] int pageSize = 24,
        [FromQuery] int? categoryId = null, [FromQuery] int? collectionId = null,
        [FromQuery] ProductBrand? brand = null, [FromQuery] string? surface = null,
        [FromQuery] string? q = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 2000);

        var result = await _productService.GetPagedAsync(new ProductQuery
        {
            PageNumber = page,
            PageSize = pageSize,
            Status = ProductStatus.Active,
            CategoryId = categoryId,
            CollectionId = collectionId,
            Brand = brand,
            Surface = string.IsNullOrWhiteSpace(surface) ? null : surface.Trim(),
            Search = string.IsNullOrWhiteSpace(q) ? null : q.Trim()
        });
        var items = result.Items
            .Select(x => new PublicProductListResponse(
                x.Id, x.ProductCode, ProductName(x.DisplayName, x.ProductCode),
                x.SeoUrl, x.ShortDescription, x.CategoryId,
                x.CategoryName, x.CollectionId, x.CollectionName,
                x.Brand.ToString(), x.BrandLabel, x.Size, x.Surface,
                x.Color, AssetUrl(x.PrimaryImagePath), x.DisplayOrder))
            .ToList();

        return Ok(new PublicPagedResponse<PublicProductListResponse>(items, result.PageNumber, result.PageSize, result.TotalCount));
    }

    [HttpGet("surfaces")]
    public async Task<ActionResult<IReadOnlyList<PublicSurfaceResponse>>> Surfaces(
        [FromQuery] string lang = "tr", [FromQuery] ProductBrand? brand = null)
    {
        if (brand == ProductBrand.NgPerforma)
            return Ok(Array.Empty<PublicSurfaceResponse>());

        var products = (await _productService.GetPagedAsync(new ProductQuery
            {
                PageNumber = 1, PageSize = 2000, Status = ProductStatus.Active, Brand = brand
            })).Items
            .Where(x => !string.IsNullOrWhiteSpace(x.PrimaryImagePath))
            .OrderBy(x => x.PrimaryImagePath?.EndsWith("/fallback/deneme.webp", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0)
            .ThenBy(x => x.DisplayOrder)
            .ToList();
        var items = (await _productService.GetActiveSurfaceSummariesAsync(brand))
            .Select(item =>
            {
                var sampleImage = products.FirstOrDefault(product =>
                    product.Brand == item.Brand &&
                    string.Equals(product.Surface?.Trim(), item.Name.Trim(), StringComparison.CurrentCultureIgnoreCase))
                    ?.PrimaryImagePath;
                return new PublicSurfaceResponse(
                    item.Name,
                    SurfaceSlug(item.Name),
                    item.Brand.ToString(),
                    ProductEnumDisplay.GetBrandLabel(item.Brand),
                    AssetUrl(sampleImage),
                    item.ProductCount);
            })
            .ToList();

        return Ok(items);
    }

    [HttpGet("products/{seoUrl}")]
    public async Task<ActionResult<PublicProductDetailResponse>> Product(string seoUrl, [FromQuery] string lang = "tr")
    {
        var x = await _productService.GetBySeoUrlAsync(seoUrl, NormalizeLanguage(lang));
        if (x is null || x.Status != ProductStatus.Active) return NotFound();
        var t = Translation(x.Translations, lang, item => item.LanguageCode);
        return Ok(new PublicProductDetailResponse
        {
            Id = x.Id, ProductCode = x.ProductCode, Name = ProductName(t?.Name, x.ProductCode),
            ShortDescription = t?.ShortDescription, LongDescription = t?.LongDescription, SeoUrl = t?.SeoUrl,
            MetaTitle = t?.MetaTitle, MetaDescription = t?.MetaDescription, CategoryId = x.CategoryId,
            CategoryName = x.CategoryName, CollectionId = x.CollectionId, CollectionName = x.CollectionName,
            Brand = x.Brand.ToString(), BrandLabel = x.BrandLabel, Status = x.Status.ToString(),
            StatusLabel = ProductEnumDisplay.GetStatusLabel(x.Status), ProductGroup = x.ProductGroup,
            Size = x.Size, Unit = x.Unit,
            Surface = x.Surface, Relief = x.Relief, SpecialSurface = x.SpecialSurface, FaceCount = x.FaceCount,
            Thickness = x.Thickness, BodyType = x.BodyType, Color = x.Color, ColorMaterial = x.ColorMaterial,
            ApplicationArea = x.ApplicationArea, UsageArea = x.UsageArea, Finish = x.Finish, Pei = x.PEI,
            VValue = x.VValue, RValue = x.RValue, DeepAbrasion = x.DeepAbrasion,
            HeatResistance = x.HeatResistance, AntiSlip = x.AntiSlip,
            GlazedGranite = x.GlazedGranite, HasFace = x.HasFace, HasVenue = x.HasVenue,
            BoxM2 = x.BoxM2, PalletM2 = x.PalletM2,
            PrimaryImageUrl = AssetUrl(x.PrimaryImagePath),
            Images = x.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).Take(ProductImageService.MaxImagesPerProduct).Select(i => new PublicProductImageResponse(
                i.Id, i.ImageType.ToString(), i.ImageTypeLabel, AssetUrl(i.FilePath)!, i.IsPrimary, i.DisplayOrder)).ToList()
        });
    }

    [HttpGet("documents")]
    public async Task<ActionResult<IReadOnlyList<PublicDocumentResponse>>> Documents(
        [FromQuery] string lang = "tr", [FromQuery] DocumentType? type = null)
    {
        var productsById = (await _productService.GetAllAsync()).ToDictionary(x => x.Id);
        var collectionsById = (await _collectionService.GetAllAsync()).ToDictionary(x => x.Id);
        var items = (await _documentService.GetAllAsync())
            .Where(x => x.IsActive && string.Equals(x.LanguageCode, NormalizeLanguage(lang), StringComparison.OrdinalIgnoreCase))
            .Where(x => !type.HasValue || x.DocumentType == type)
            .OrderBy(x => x.DisplayOrder)
            .Select(x =>
            {
                var relatedProducts = x.RelatedProducts
                    .Select(item => productsById.GetValueOrDefault(item.Id))
                    .Where(item => item is not null)
                    .ToList();
                var collectionIds = x.RelatedCollections.Select(item => item.Id)
                    .Concat(relatedProducts.Select(item => item!.CollectionId))
                    .Distinct()
                    .ToList();
                var relatedCollections = collectionIds
                    .Select(id => collectionsById.GetValueOrDefault(id))
                    .Where(item => item is not null)
                    .ToList();
                var brands = new[] { x.Brand }
                    .Select(brand => new PublicDocumentFilterOptionResponse(
                        brand.ToString(), ProductEnumDisplay.GetBrandLabel(brand)))
                    .ToList();
                var collections = relatedCollections
                    .OrderBy(item => item!.DisplayName)
                    .Select(item => new PublicDocumentFilterOptionResponse(
                        item!.Id.ToString(), item.DisplayName ?? $"#{item.Id}"))
                    .ToList();
                return new PublicDocumentResponse(
                    x.Id, x.Title, x.Description, x.DocumentType.ToString(), x.DocumentTypeLabel, x.LanguageCode,
                    AssetUrl(x.FilePath)!, x.OriginalFileName, x.ContentType, x.FileSize, x.FileSizeLabel, x.DisplayOrder,
                    brands, collections);
            })
            .ToList();
        return Ok(items);
    }

    [HttpGet("documents/{id:int}/download")]
    public async Task<IActionResult> DownloadDocument(int id)
    {
        var document = await _documentService.GetByIdAsync(id);
        if (document is null || !document.IsActive)
        {
            return NotFound();
        }

        Stream stream;
        try
        {
            stream = await _fileStorageService.OpenReadAsync(document.FilePath);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }

        return File(stream, document.ContentType, document.OriginalFileName);
    }

    [HttpGet("projects")]
    public async Task<ActionResult<IReadOnlyList<PublicReferenceProjectListResponse>>> Projects([FromQuery] string lang = "tr")
    {
        var items = (await _referenceProjectService.GetAllAsync())
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .Select(x =>
            {
                var t = Translation(x.Translations, lang, y => y.LanguageCode);
                return new PublicReferenceProjectListResponse(
                    x.Id, t?.ProjectName ?? x.DisplayName ?? string.Empty, t?.Description, t?.SeoUrl,
                    x.Location, x.Region.ToString(), x.RegionLabel, x.Brand.ToString(), x.BrandLabel,
                    x.ProjectType.ToString(), x.ProjectTypeLabel, x.Architect, x.Year,
                    AssetUrl(x.FeaturedImagePath), x.DisplayOrder);
            }).ToList();
        return Ok(items);
    }

    [HttpGet("project-filter-options")]
    public ActionResult<PublicReferenceProjectFilterOptionsResponse> ProjectFilterOptions() => Ok(
        new PublicReferenceProjectFilterOptionsResponse(
            Enum.GetValues<ReferenceProjectRegion>().Select(value => new PublicReferenceProjectOptionResponse(value.ToString(), ReferenceProjectEnumDisplay.GetRegionLabel(value))).ToList(),
            Enum.GetValues<ProductBrand>().Select(value => new PublicReferenceProjectOptionResponse(value.ToString(), ReferenceProjectEnumDisplay.GetBrandLabel(value))).ToList(),
            Enum.GetValues<ReferenceProjectType>().Select(value => new PublicReferenceProjectOptionResponse(value.ToString(), ReferenceProjectEnumDisplay.GetProjectTypeLabel(value))).ToList()));

    [HttpGet("projects/{seoUrl}")]
    public async Task<ActionResult<PublicReferenceProjectDetailResponse>> Project(string seoUrl, [FromQuery] string lang = "tr")
    {
        var hasNumericId = int.TryParse(seoUrl, out var numericId);
        var project = (await _referenceProjectService.GetAllAsync())
            .Where(x => x.IsActive)
            .Select(x => (Project: x, Translation: Translation(x.Translations, lang, t => t.LanguageCode)))
            .FirstOrDefault(x => string.Equals(x.Translation?.SeoUrl, seoUrl, StringComparison.OrdinalIgnoreCase)
                || (hasNumericId && x.Project.Id == numericId));
        if (project.Project is null) return NotFound();

        var images = await _referenceProjectImageService.GetByReferenceProjectIdAsync(project.Project.Id);
        return Ok(new PublicReferenceProjectDetailResponse
        {
            Id = project.Project.Id, Name = project.Translation?.ProjectName ?? project.Project.DisplayName ?? string.Empty,
            Description = project.Translation?.Description, SeoUrl = project.Translation?.SeoUrl,
            Location = project.Project.Location, ProjectType = project.Project.ProjectType.ToString(),
            ProjectTypeLabel = project.Project.ProjectTypeLabel, Architect = project.Project.Architect,
            Year = project.Project.Year, FeaturedImageUrl = AssetUrl(project.Project.FeaturedImagePath),
            Images = images.OrderBy(x => x.DisplayOrder).Select(x => new PublicReferenceProjectImageResponse(
                x.Id, AssetUrl(x.FilePath)!, x.IsFeatured, x.DisplayOrder)).ToList(),
            RelatedProductIds = project.Project.RelatedProducts.Select(x => x.Id).ToList(),
            RelatedProducts = project.Project.RelatedProducts.Select(x => new PublicReferenceProjectProductResponse(
                x.Id, x.Label, x.Name, x.SeoUrl)).ToList()
        });
    }

    private bool HasUsableImage(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        if (Uri.TryCreate(path, UriKind.Absolute, out var absolute))
            return absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps;
        var normalizedPath = $"/{path.Replace('\\', '/').TrimStart('/')}";
        if (normalizedPath.StartsWith("/uploads/products/imported/", StringComparison.OrdinalIgnoreCase))
            return true;
        var storagePath = normalizedPath;
        return _fileStorageService.Exists(storagePath);
    }

    private string? AssetUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        if (Uri.TryCreate(path, UriKind.Absolute, out var absolute)
            && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
        {
            return absolute.ToString();
        }
        return $"{Request.Scheme}://{Request.Host}{Request.PathBase}/{path.TrimStart('/')}";
    }

    private static string NormalizeLanguage(string? language) =>
        string.IsNullOrWhiteSpace(language) ? "TR" : language.Trim().ToUpperInvariant();

    private static string ProductName(string? name, string productCode) =>
        string.IsNullOrWhiteSpace(name) ? productCode : name.Trim();

    private static string SurfaceSlug(string value)
    {
        var normalized = value.Trim().ToLower(new System.Globalization.CultureInfo("tr-TR"))
            .Replace('ı', 'i').Replace('ğ', 'g').Replace('ü', 'u')
            .Replace('ş', 's').Replace('ö', 'o').Replace('ç', 'c');
        return System.Text.RegularExpressions.Regex.Replace(normalized, @"[^a-z0-9]+", "-").Trim('-');
    }

    private static T? Translation<T>(IEnumerable<T> translations, string language, Func<T, string> languageCode)
        where T : class
    {
        var items = translations.ToList();
        var requested = NormalizeLanguage(language);
        return items.FirstOrDefault(x => string.Equals(languageCode(x), requested, StringComparison.OrdinalIgnoreCase))
            ?? items.FirstOrDefault(x => string.Equals(languageCode(x), "TR", StringComparison.OrdinalIgnoreCase))
            ?? items.FirstOrDefault();
    }
}
