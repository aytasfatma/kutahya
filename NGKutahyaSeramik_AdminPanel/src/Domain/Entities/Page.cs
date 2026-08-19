namespace Domain.Entities;

/// <summary>
/// Madde 16.2/17.2 — Kurumsal sayfalar (Hakkımızda, Tarihçe, Kariyer vb.), serbest içerik listesi
/// (Blog/News gibi — kapalı bir sayfa tipi enum'u değil). Title/SeoUrl/MetaTitle/MetaDescription
/// tamamen Translation'da (EntityType.Page). Dokümanda Page için IsActive/Status/PublishDate/ParentId/
/// DisplayOrder hiç geçmiyor (bu, projede ilk kez rastlanan bir sessizlik — Category/Collection/
/// ReferenceProject/Banner'ın hepsinde "aktif/pasif" açıkça vardı) — bu yüzden hiçbiri eklenmedi;
/// kaldırma yalnızca hard-delete (Madde 30 RBAC'ı İçerik Editörü'ne zaten "CRUD" — silme dahil — veriyor).
/// </summary>
public class Page
{
    public int Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Page()
    {
        var now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public void Touch() => UpdatedAt = DateTime.UtcNow;
}
