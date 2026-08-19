using Domain.Enums;

namespace Presentation.Models.Api;

public sealed record PublicPagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public sealed record PublicCategoryResponse(
    int Id, int? ParentCategoryId, string Name, string? Description, string? SeoUrl,
    string? MetaTitle, string? MetaDescription, string? ImageUrl, int DisplayOrder,
    IReadOnlyList<ProductBrand> Brands, int ProductCount);

public sealed record PublicCollectionResponse(
    int Id, string Name, string? Description, string? SeoUrl, string? MetaTitle,
    string? MetaDescription, string? ImageUrl, int ProductCount, int DisplayOrder,
    IReadOnlyList<ProductBrand> Brands);

public sealed record PublicSurfaceResponse(
    string Name, string Slug, string Brand, string BrandLabel,
    string? ImageUrl, int ProductCount);

public sealed record PublicProductListResponse(
    int Id, string ProductCode, string Name, string? SeoUrl, string? ShortDescription,
    int? CategoryId, string? CategoryName, int CollectionId, string? CollectionName,
    string Brand, string BrandLabel, string Size, string? Surface, string? Color,
    string? PrimaryImageUrl, int DisplayOrder);

public sealed record PublicProductImageResponse(
    int Id, string ImageType, string ImageTypeLabel, string Url, bool IsPrimary, int DisplayOrder);

public sealed class PublicProductDetailResponse
{
    public int Id { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? ShortDescription { get; init; }
    public string? LongDescription { get; init; }
    public string? SeoUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public int? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public int CollectionId { get; init; }
    public string? CollectionName { get; init; }
    public string Brand { get; init; } = string.Empty;
    public string BrandLabel { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string StatusLabel { get; init; } = string.Empty;
    public string? ProductGroup { get; init; }
    public string Size { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public string? Surface { get; init; }
    public string? Relief { get; init; }
    public string? SpecialSurface { get; init; }
    public int? FaceCount { get; init; }
    public decimal? Thickness { get; init; }
    public string? BodyType { get; init; }
    public string? Color { get; init; }
    public string? ColorMaterial { get; init; }
    public string? ApplicationArea { get; init; }
    public string? UsageArea { get; init; }
    public string? Finish { get; init; }
    public decimal? Pei { get; init; }
    public string? VValue { get; init; }
    public string? RValue { get; init; }
    public string? DeepAbrasion { get; init; }
    public bool? HeatResistance { get; init; }
    public bool? AntiSlip { get; init; }
    public bool? GlazedGranite { get; init; }
    public bool? HasFace { get; init; }
    public bool? HasVenue { get; init; }
    public decimal? BoxM2 { get; init; }
    public decimal? PalletM2 { get; init; }
    public string? PrimaryImageUrl { get; init; }
    public IReadOnlyList<PublicProductImageResponse> Images { get; init; } = [];
}

public sealed record PublicDocumentFilterOptionResponse(string Value, string Label);

public sealed record PublicDocumentResponse(
    int Id, string Title, string? Description, string DocumentType, string DocumentTypeLabel, string LanguageCode,
    string FileUrl, string OriginalFileName, string ContentType, long FileSize,
    string FileSizeLabel, int DisplayOrder,
    IReadOnlyList<PublicDocumentFilterOptionResponse> Brands,
    IReadOnlyList<PublicDocumentFilterOptionResponse> Collections);

public sealed record PublicReferenceProjectListResponse(
    int Id, string Name, string? Description, string? SeoUrl, string? Location,
    string Region, string RegionLabel, string Brand, string BrandLabel,
    string ProjectType, string ProjectTypeLabel, string? Architect, int? Year,
    string? FeaturedImageUrl, int DisplayOrder);

public sealed record PublicReferenceProjectOptionResponse(string Value, string Label);
public sealed record PublicReferenceProjectFilterOptionsResponse(
    IReadOnlyList<PublicReferenceProjectOptionResponse> Regions,
    IReadOnlyList<PublicReferenceProjectOptionResponse> Brands,
    IReadOnlyList<PublicReferenceProjectOptionResponse> ProjectTypes);

public sealed record PublicReferenceProjectImageResponse(int Id, string Url, bool IsFeatured, int DisplayOrder);
public sealed record PublicReferenceProjectProductResponse(int Id, string ProductCode, string Name, string SeoUrl);

public sealed class PublicReferenceProjectDetailResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? SeoUrl { get; init; }
    public string? Location { get; init; }
    public string ProjectType { get; init; } = string.Empty;
    public string ProjectTypeLabel { get; init; } = string.Empty;
    public string? Architect { get; init; }
    public int? Year { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public IReadOnlyList<PublicReferenceProjectImageResponse> Images { get; init; } = [];
    public IReadOnlyList<int> RelatedProductIds { get; init; } = [];
    public IReadOnlyList<PublicReferenceProjectProductResponse> RelatedProducts { get; init; } = [];
}
