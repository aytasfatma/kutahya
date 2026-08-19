namespace Domain.Entities;

/// <summary>
/// Madde 36.1 "NewsCategories" — BlogCategory ile birebir aynı desen: düz/hiyerarşisiz, Translation-
/// tabanlı Name (Madde 28.2 "kategori adı" çoklu dil gerektiren alanlar arasında). Madde 22'nin verdiği
/// 6 sabit kategori adı (Ödüller/Sürdürülebilirlik/Sertifikalar/Kutlamalar/Bültenler/Kurumsal)
/// bilinçli olarak seed edilmedi — CategorySeeder/CollectionSeeder emsaliyle tutarlı.
/// </summary>
public class NewsCategory
{
    public int Id { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    private NewsCategory()
    {
    }

    public NewsCategory(int displayOrder)
    {
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public void UpdateDetails(int displayOrder) => DisplayOrder = displayOrder;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
