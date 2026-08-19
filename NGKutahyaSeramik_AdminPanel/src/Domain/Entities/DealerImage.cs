namespace Domain.Entities;

public class DealerImage
{
    public int Id { get; private set; }
    public int DealerId { get; private set; }
    public Dealer Dealer { get; private set; } = null!;
    public string FilePath { get; private set; } = string.Empty;
    public bool IsFeatured { get; private set; }
    public int DisplayOrder { get; private set; }

    private DealerImage() { }

    public DealerImage(int dealerId, string filePath, bool isFeatured, int displayOrder)
    {
        DealerId = dealerId;
        FilePath = filePath;
        IsFeatured = isFeatured;
        DisplayOrder = displayOrder;
    }

    public void SetFeatured(bool value) => IsFeatured = value;
    public void UpdateDisplayOrder(int value) => DisplayOrder = value;
}
