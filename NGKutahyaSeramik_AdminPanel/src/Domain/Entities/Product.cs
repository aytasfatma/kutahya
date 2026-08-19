using Domain.Enums;

namespace Domain.Entities;

public class Product
{
    public int Id { get; private set; }
    public string ProductCode { get; private set; } = null!;

    public int? CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public int CollectionId { get; private set; }
    public Collection Collection { get; private set; } = null!;

    public int? SurfaceId { get; private set; }
    public Surface? SurfaceDefinition { get; private set; }

    public ProductBrand Brand { get; private set; }
    public string BrandValues { get; private set; } = string.Empty;
    public IReadOnlyList<ProductBrand> Brands => ParseBrands(BrandValues, Brand);
    public ProductStatus Status { get; private set; }

    public string? CommercialName { get; private set; }
    public string? ProductGroup { get; private set; }
    public string Size { get; private set; } = null!;
    public string Unit { get; private set; } = null!;
    public string? Surface { get; private set; }
    public string? Relief { get; private set; }
    public string? SpecialSurface { get; private set; }
    public int? FaceCount { get; private set; }
    public decimal? Thickness { get; private set; }
    public string? BodyType { get; private set; }
    public string? Color { get; private set; }
    public string? ColorMaterial { get; private set; }
    public string? ApplicationArea { get; private set; }
    public string? UsageArea { get; private set; }
    public string? Finish { get; private set; }
    public decimal? PEI { get; private set; }
    public string? VValue { get; private set; }
    public string? RValue { get; private set; }
    public string? DeepAbrasion { get; private set; }
    public bool? HeatResistance { get; private set; }
    public bool? AntiSlip { get; private set; }
    public bool? GlazedGranite { get; private set; }
    public bool? HasFace { get; private set; }
    public bool? HasVenue { get; private set; }
    public decimal? BoxM2 { get; private set; }
    public decimal? PalletM2 { get; private set; }

    public int DisplayOrder { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Product()
    {
    }

    public Product(
        string productCode,
        int? categoryId,
        int collectionId,
        ProductBrand brand,
        ProductStatus status,
        string? commercialName,
        string? productGroup,
        string size,
        string unit,
        string? surface,
        string? relief,
        string? specialSurface,
        int? faceCount,
        decimal? thickness,
        string? bodyType,
        string? color,
        string? colorMaterial,
        string? applicationArea,
        string? usageArea,
        string? finish,
        decimal? pei,
        string? vValue,
        string? rValue,
        string? deepAbrasion,
        bool? heatResistance,
        bool? antiSlip,
        bool? glazedGranite,
        bool? hasFace,
        bool? hasVenue,
        decimal? boxM2,
        decimal? palletM2,
        int displayOrder,
        IReadOnlyCollection<ProductBrand>? brands = null)
    {
        ProductCode = productCode;
        CategoryId = categoryId;
        CollectionId = collectionId;
        SetBrands(brands is { Count: > 0 } ? brands : [brand]);
        Status = status;
        CommercialName = commercialName;
        ProductGroup = productGroup;
        Size = size;
        Unit = unit;
        Surface = surface;
        Relief = relief;
        SpecialSurface = specialSurface;
        FaceCount = faceCount;
        Thickness = thickness;
        BodyType = bodyType;
        Color = color;
        ColorMaterial = colorMaterial;
        ApplicationArea = applicationArea;
        UsageArea = usageArea;
        Finish = finish;
        PEI = pei;
        VValue = vValue;
        RValue = rValue;
        DeepAbrasion = deepAbrasion;
        HeatResistance = heatResistance;
        AntiSlip = antiSlip;
        GlazedGranite = glazedGranite;
        HasFace = hasFace;
        HasVenue = hasVenue;
        BoxM2 = boxM2;
        PalletM2 = palletM2;
        DisplayOrder = displayOrder;

        var now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public void UpdateDetails(
        string productCode,
        int? categoryId,
        int collectionId,
        ProductBrand brand,
        ProductStatus status,
        string? commercialName,
        string? productGroup,
        string size,
        string unit,
        string? surface,
        string? relief,
        string? specialSurface,
        int? faceCount,
        decimal? thickness,
        string? bodyType,
        string? color,
        string? colorMaterial,
        string? applicationArea,
        string? usageArea,
        string? finish,
        decimal? pei,
        string? vValue,
        string? rValue,
        string? deepAbrasion,
        bool? heatResistance,
        bool? antiSlip,
        bool? glazedGranite,
        bool? hasFace,
        bool? hasVenue,
        decimal? boxM2,
        decimal? palletM2,
        int displayOrder,
        IReadOnlyCollection<ProductBrand>? brands = null)
    {
        ProductCode = productCode;
        CategoryId = categoryId;
        CollectionId = collectionId;
        SetBrands(brands is { Count: > 0 } ? brands : [brand]);
        Status = status;
        CommercialName = commercialName;
        ProductGroup = productGroup;
        Size = size;
        Unit = unit;
        Surface = surface;
        Relief = relief;
        SpecialSurface = specialSurface;
        FaceCount = faceCount;
        Thickness = thickness;
        BodyType = bodyType;
        Color = color;
        ColorMaterial = colorMaterial;
        ApplicationArea = applicationArea;
        UsageArea = usageArea;
        Finish = finish;
        PEI = pei;
        VValue = vValue;
        RValue = rValue;
        DeepAbrasion = deepAbrasion;
        HeatResistance = heatResistance;
        AntiSlip = antiSlip;
        GlazedGranite = glazedGranite;
        HasFace = hasFace;
        HasVenue = hasVenue;
        BoxM2 = boxM2;
        PalletM2 = palletM2;
        DisplayOrder = displayOrder;

        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSurface(int? surfaceId, string? surfaceName)
    {
        SurfaceId = surfaceId;
        Surface = string.IsNullOrWhiteSpace(surfaceName) ? null : surfaceName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetBrands(IEnumerable<ProductBrand> brands)
    {
        var values = brands.Distinct().OrderBy(value => value).ToArray();
        if (values.Length == 0)
        {
            values = [Brand];
        }

        Brand = values[0];
        BrandValues = string.Join(',', values.Select(value => value.ToString()));
    }

    private static IReadOnlyList<ProductBrand> ParseBrands(string? values, ProductBrand fallback)
    {
        var parsed = (values ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => Enum.TryParse<ProductBrand>(value, true, out var brand) ? (ProductBrand?)brand : null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .Distinct()
            .ToArray();

        return parsed.Length > 0 ? parsed : [fallback];
    }
}
