using Domain.Enums;

namespace Application.Forms;

/// <summary>Tek istek tipi — üç form türünün ortak+tip-özel alanlarını taşır; hangi alanların
/// zorunlu olduğu FormType'a göre serviste doğrulanır (Madde 29.1/29.2/29.3). Bu servis metodu
/// bu task'ta hiçbir public/anonim controller'dan çağrılmıyor (ADR-001/002/009: public site bu
/// fazın kapsamı dışı) — yalnızca gelecekteki public form gönderimi için hazır, testlerde
/// doğrudan çağrılıyor.</summary>
public class CreateFormSubmissionRequest
{
    public FormType FormType { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? Company { get; init; }
    public string Message { get; init; } = string.Empty;
    public bool ConsentAccepted { get; init; }

    public string? Subject { get; init; }

    public string? ProductCode { get; init; }
    public string? ProductName { get; init; }

    public string? Address { get; init; }
    public string? RequestedProduct { get; init; }
    public int? Quantity { get; init; }
}
