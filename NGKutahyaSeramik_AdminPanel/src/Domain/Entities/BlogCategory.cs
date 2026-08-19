namespace Domain.Entities;

/// <summary>
/// Madde 36.1 "BlogCategories" — Product'ın Kategori Yönetimi'nden (Task 4.1) tamamen bağımsız,
/// düz/hiyerarşisiz bir taksonomi (doküman blog kategorileri için hiyerarşi işareti vermiyor,
/// Haber kategorileri de — Madde 22 — düz bir liste). Category ile aynı Translation-tabanlı
/// Name deseni (Madde 28.2 "kategori adı" çoklu dil gerektiren alanlar arasında sayılıyor).
/// </summary>
public class BlogCategory
{
    public int Id { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    private BlogCategory()
    {
    }

    public BlogCategory(int displayOrder)
    {
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public void UpdateDetails(int displayOrder) => DisplayOrder = displayOrder;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
