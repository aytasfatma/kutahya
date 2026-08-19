using Domain.Enums;

namespace Application.ProductImages;

public static class ProductImageEnumDisplay
{
    public static string GetImageTypeLabel(ProductImageType imageType) => imageType switch
    {
        ProductImageType.Render => "Render",
        ProductImageType.Face => "Face",
        ProductImageType.Lifestyle => "Lifestyle",
        ProductImageType.Texture => "Texture",
        ProductImageType.Detail => "Detay",
        _ => imageType.ToString()
    };
}
