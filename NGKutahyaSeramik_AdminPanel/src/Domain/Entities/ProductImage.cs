using Domain.Enums;

namespace Domain.Entities;

public class ProductImage
{
    public int Id { get; private set; }
    public int ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public ProductImageType ImageType { get; private set; }
    public string FilePath { get; private set; } = null!;
    public bool IsPrimary { get; private set; }
    public int DisplayOrder { get; private set; }

    private ProductImage()
    {
    }

    public ProductImage(int productId, ProductImageType imageType, string filePath, bool isPrimary, int displayOrder)
    {
        ProductId = productId;
        ImageType = imageType;
        FilePath = filePath;
        IsPrimary = isPrimary;
        DisplayOrder = displayOrder;
    }

    public void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;

    public void UpdateDisplayOrder(int displayOrder) => DisplayOrder = displayOrder;
}
