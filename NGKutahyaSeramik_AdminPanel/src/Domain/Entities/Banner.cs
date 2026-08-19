namespace Domain.Entities;

/// <summary>
/// Madde 16.1 — "Hero Banner yönetim panelinden yönetilebilir olacaktır: görsel/video yükleme,
/// başlık, alt başlık, CTA butonu metni ve linki, sıralama, aktif/pasif durumu, yayın tarihi aralığı."
/// Video yükleme bu task'ta kapsam dışı bırakıldı (mevcut görsel-doğrulama altyapısı video formatlarını
/// kapsamıyor). SEO alanları hiç eklenmedi — Madde 17.2/36.1 Banner için SEO anmıyor. Blog/News'in
/// aksine burada Status enum değil, doküman açıkça "aktif/pasif" (2 durum) istediği için bool IsActive
/// kullanıldı (Category/Collection/ReferenceProject deseni). Yayın tarihi de Blog/News'in tekil
/// PublishDate'inden farklı — doküman "aralık" dediği için Start/End çifti.
/// </summary>
public class Banner
{
    public int Id { get; private set; }
    public string? ImagePath { get; private set; }
    public DateTime? PublishStartDate { get; private set; }
    public DateTime? PublishEndDate { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    private Banner()
    {
    }

    public Banner(DateTime? publishStartDate, DateTime? publishEndDate, int displayOrder)
    {
        PublishStartDate = publishStartDate;
        PublishEndDate = publishEndDate;
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public void UpdateDetails(DateTime? publishStartDate, DateTime? publishEndDate, int displayOrder)
    {
        PublishStartDate = publishStartDate;
        PublishEndDate = publishEndDate;
        DisplayOrder = displayOrder;
    }

    public void SetImagePath(string? filePath) => ImagePath = filePath;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
