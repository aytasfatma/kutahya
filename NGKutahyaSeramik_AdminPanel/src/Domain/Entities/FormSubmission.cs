using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Madde 17.2 ("Form Yönetimi | İletişim ve bilgi talep formları listesi, durum takibi, e-posta
/// bildirimleri") + Madde 29 (Formlar). Tek tablo + ortak alanlar + tip-bazlı opsiyonel alanlar
/// deseni seçildi (Seçenek 1) — yalnızca 3 form türü var, tip-özel alanlar (Subject; ProductCode/
/// ProductName; Address/RequestedProduct/Quantity) birkaç skaler alandan ibaret, ayrı detay
/// entity'leri veya JSON blob'u haklı çıkaracak bir karmaşıklık/veri hacmi yok. Admin panelinin
/// "tek gelen kutusu" ihtiyacı (Madde 17.2) da tek tabloyu destekliyor.
/// Translation KULLANILMIYOR — tüm alanlar kullanıcı tarafından girilen ham veri (ad/e-posta/mesaj/
/// firma/adres), admin tarafından yönetilen çevrilebilir bir başlık/etiket yok (Dealer'dan sonra
/// Translation'ı hiç tüketmeyen ikinci modül).
/// Durum: Madde 17.2 yalnızca soyut "durum takibi" diyor, somut değer listesi (Blog/News/Product'taki
/// gibi) vermiyor — bu yüzden icat edilmiş bir Status enum yerine, okuma/işleme aşamalarını zaman
/// damgasıyla temsil eden IsRead/ReadAt/ProcessedAt kullanıldı (ProcessedAt dolu olması "işleme
/// alındı" anlamına gelir — Banner'ın PublishStartDate/EndDate nullable-zaman-damgası deseniyle
/// tutarlı, ekstra bir bool bayrağı gerektirmiyor).
/// </summary>
public class FormSubmission
{
    public int Id { get; private set; }
    public FormType FormType { get; private set; }

    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? Company { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public bool ConsentAccepted { get; private set; }

    // Yalnızca FormType.Contact
    public string? Subject { get; private set; }

    // Yalnızca FormType.RequestInformation (Madde 29.2: "Ürün kodu ve adı otomatik olarak form
    // verisine eklenir")
    public string? ProductCode { get; private set; }
    public string? ProductName { get; private set; }

    // Yalnızca FormType.SampleRequest (Madde 29.3)
    public string? Address { get; private set; }
    public string? RequestedProduct { get; private set; }
    public int? Quantity { get; private set; }

    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? AdminNote { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private FormSubmission()
    {
    }

    public FormSubmission(
        FormType formType,
        string fullName,
        string email,
        string phone,
        string? company,
        string message,
        bool consentAccepted,
        string? subject,
        string? productCode,
        string? productName,
        string? address,
        string? requestedProduct,
        int? quantity)
    {
        FormType = formType;
        FullName = fullName;
        Email = email;
        Phone = phone;
        Company = company;
        Message = message;
        ConsentAccepted = consentAccepted;
        Subject = subject;
        ProductCode = productCode;
        ProductName = productName;
        Address = address;
        RequestedProduct = requestedProduct;
        Quantity = quantity;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }

    public void MarkAsUnread()
    {
        IsRead = false;
        ReadAt = null;
    }

    public void MarkAsProcessed() => ProcessedAt = DateTime.UtcNow;

    public void MarkAsUnprocessed() => ProcessedAt = null;

    public void UpdateAdminNote(string? note) => AdminNote = note;
}
