using Domain.Enums;

namespace Application.Products;

public class ProductTranslationInput
{
    public int LanguageId { get; init; }
    public string? Name { get; init; }
    public string? ShortDescription { get; init; }
    public string? LongDescription { get; init; }
    public string? SeoUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}

public class ProductRequestBase
{
    public string ProductCode { get; init; } = string.Empty;
    public int? CategoryId { get; init; }
    public int CollectionId { get; init; }
    public int? SurfaceId { get; init; }
    public ProductBrand Brand { get; init; }
    public IReadOnlyList<ProductBrand> Brands { get; init; } = Array.Empty<ProductBrand>();
    public ProductStatus Status { get; init; }
    public string? CommercialName { get; init; }
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
    public decimal? PEI { get; init; }
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
    public int DisplayOrder { get; init; }
    public IReadOnlyList<ProductTranslationInput> Translations { get; init; } = Array.Empty<ProductTranslationInput>();
}

public class CreateProductRequest : ProductRequestBase
{
}

public class UpdateProductRequest : ProductRequestBase
{
}
