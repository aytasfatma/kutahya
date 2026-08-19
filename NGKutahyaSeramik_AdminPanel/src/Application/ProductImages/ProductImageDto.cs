using Domain.Enums;

namespace Application.ProductImages;

public class ProductImageDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public ProductImageType ImageType { get; init; }
    public string FilePath { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
    public int DisplayOrder { get; init; }

    public string ImageTypeLabel => ProductImageEnumDisplay.GetImageTypeLabel(ImageType);
}
