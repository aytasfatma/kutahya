using Domain.Enums;

namespace Application.Products;

public static class ProductEnumDisplay
{
    public static string GetBrandLabel(ProductBrand brand) => brand switch
    {
        ProductBrand.NgSeramik => "NG SERAMİK",
        ProductBrand.NgStone => "NG STONE",
        ProductBrand.NgSlim => "NG SLIM",
        ProductBrand.NgPerforma => "NG PERFORMA",
        _ => brand.ToString()
    };

    public static string GetStatusLabel(ProductStatus status) => status switch
    {
        ProductStatus.Active => "Aktif",
        ProductStatus.Inactive => "Pasif",
        ProductStatus.InProgress => "Devam",
        ProductStatus.Cancelled => "İptal",
        _ => status.ToString()
    };
}
