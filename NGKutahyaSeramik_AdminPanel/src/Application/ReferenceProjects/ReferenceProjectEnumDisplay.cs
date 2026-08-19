using Domain.Enums;

namespace Application.ReferenceProjects;

public static class ReferenceProjectEnumDisplay
{
    public static string GetRegionLabel(ReferenceProjectRegion region) => region switch
    {
        ReferenceProjectRegion.Domestic => "Yurt İçi",
        ReferenceProjectRegion.International => "Yurt Dışı",
        _ => region.ToString()
    };

    public static string GetBrandLabel(ProductBrand brand) => brand switch
    {
        ProductBrand.NgSeramik => "NG Seramik",
        ProductBrand.NgStone => "NG Stone",
        ProductBrand.NgSlim => "NG Slim",
        ProductBrand.NgPerforma => "NG Performa",
        _ => brand.ToString()
    };

    public static string GetProjectTypeLabel(ReferenceProjectType type) => type switch
    {
        ReferenceProjectType.Konut => "Konut",
        ReferenceProjectType.Otel => "Otel",
        ReferenceProjectType.Ofis => "Ofis",
        ReferenceProjectType.Avm => "AVM",
        ReferenceProjectType.Hastane => "Hastane",
        ReferenceProjectType.DisMekan => "Dış Mekan",
        _ => type.ToString()
    };
}
