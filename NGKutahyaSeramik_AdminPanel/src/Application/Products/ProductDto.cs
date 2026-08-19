using Application.ProductImages;
using Domain.Enums;

namespace Application.Products;

public class ProductTranslationDto
{
    public int LanguageId { get; init; }
    public string LanguageCode { get; init; } = string.Empty;
    public string LanguageName { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? ShortDescription { get; init; }
    public string? LongDescription { get; init; }
    public string? SeoUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}

public class ProductDto
{
    public int Id { get; init; }
    public string ProductCode { get; init; } = string.Empty;

    public int? CategoryId { get; init; }
    public string? CategoryName { get; init; }

    public int CollectionId { get; init; }
    public string? CollectionName { get; init; }
    public int? SurfaceId { get; init; }

    public string? PrimaryImagePath { get; init; }
    public IReadOnlyList<ProductImageDto> Images { get; init; } = Array.Empty<ProductImageDto>();

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

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public IReadOnlyList<ProductTranslationDto> Translations { get; init; } = Array.Empty<ProductTranslationDto>();

    public string? DisplayName =>
        Translations.FirstOrDefault(t => t.LanguageCode == "TR")?.Name;

    public string BrandLabel => string.Join(", ", Brands.DefaultIfEmpty(Brand).Select(ProductEnumDisplay.GetBrandLabel));

    public string StatusLabel => ProductEnumDisplay.GetStatusLabel(Status);
}

public class ProductListItemDto
{
    public int Id { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? SeoUrl { get; init; }
    public string? ShortDescription { get; init; }
    public int? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public int CollectionId { get; init; }
    public string? CollectionName { get; init; }
    public string? PrimaryImagePath { get; init; }
    public ProductBrand Brand { get; init; }
    public IReadOnlyList<ProductBrand> Brands { get; init; } = Array.Empty<ProductBrand>();
    public string BrandValues { get; init; } = string.Empty;
    public ProductStatus Status { get; init; }
    public string Size { get; init; } = string.Empty;
    public string? Surface { get; init; }
    public string? Color { get; init; }
    public int DisplayOrder { get; init; }

    public string BrandLabel
    {
        get
        {
            var values = Brands.Count > 0
                ? Brands
                : BrandValues.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(value => Enum.TryParse<ProductBrand>(value, true, out var brand) ? (ProductBrand?)brand : null)
                    .Where(value => value.HasValue)
                    .Select(value => value!.Value)
                    .DefaultIfEmpty(Brand)
                    .ToArray();
            return string.Join(", ", values.Select(ProductEnumDisplay.GetBrandLabel));
        }
    }

    public string StatusLabel => ProductEnumDisplay.GetStatusLabel(Status);
}

public sealed record ProductSurfaceSummaryDto(
    string Name, ProductBrand Brand, int ProductCount);

public class ProductTechnicalOptionsDto
{
    public IReadOnlyList<string> Sizes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Units { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Surfaces { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Reliefs { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SpecialSurfaces { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> BodyTypes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Colors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ColorMaterials { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ApplicationAreas { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> UsageAreas { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Finishes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> DeepAbrasions { get; init; } = Array.Empty<string>();
    public IReadOnlyList<decimal> BoxM2Values { get; init; } = Array.Empty<decimal>();
    public IReadOnlyList<decimal> PalletM2Values { get; init; } = Array.Empty<decimal>();
}
