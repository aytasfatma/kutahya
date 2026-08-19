namespace Domain.Entities;

/// <summary>
/// Madde 23.1 — "Images" (galeri) + "FeaturedImage" (kapak) tek tabloda birleştirildi: ProductImage'ın
/// IsPrimary desenindeki gibi IsFeatured bayrağı kapak görselini işaretler (ADR-013 deseninin tekrar
/// kullanımı — ayrı bir FeaturedImage sütunu/tablosu yerine).
/// </summary>
public class ReferenceProjectImage
{
    public int Id { get; private set; }
    public int ReferenceProjectId { get; private set; }
    public ReferenceProject ReferenceProject { get; private set; } = null!;
    public string FilePath { get; private set; } = null!;
    public bool IsFeatured { get; private set; }
    public int DisplayOrder { get; private set; }

    private ReferenceProjectImage()
    {
    }

    public ReferenceProjectImage(int referenceProjectId, string filePath, bool isFeatured, int displayOrder)
    {
        ReferenceProjectId = referenceProjectId;
        FilePath = filePath;
        IsFeatured = isFeatured;
        DisplayOrder = displayOrder;
    }

    public void SetFeatured(bool isFeatured) => IsFeatured = isFeatured;

    public void UpdateDisplayOrder(int displayOrder) => DisplayOrder = displayOrder;
}
