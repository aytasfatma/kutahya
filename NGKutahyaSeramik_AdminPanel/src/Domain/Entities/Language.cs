namespace Domain.Entities;

public class Language
{
    public int Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }

    public ICollection<Translation> Translations { get; private set; } = new List<Translation>();

    private Language()
    {
    }

    public Language(string code, string name, bool isActive, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Language code boş veya whitespace olamaz.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Language name boş veya whitespace olamaz.", nameof(name));
        }

        Code = code;
        Name = name;
        IsActive = isActive;
        DisplayOrder = displayOrder;
    }

    // Backlog #3 (Dil Yönetimi panel modülü) — Code kasıtlı olarak burada değiştirilemez (dil kodu
    // salt okunur); yalnızca Name/DisplayOrder/IsActive güncellenebilir. "Türkçe devre dışı
    // bırakılamaz" guardrail'i burada değil Application katmanında (LanguageService) uygulanır —
    // bu, hangi dilin "varsayılan/zorunlu" sayıldığına dair bir iş kuralıdır, entity invariant'ı değil.
    public void UpdateDetails(string name, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Language name boş veya whitespace olamaz.", nameof(name));
        }

        Name = name;
        DisplayOrder = displayOrder;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
