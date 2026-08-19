namespace Domain.Entities;

/// <summary>
/// Madde 21.1 "Tags: array" — doküman bu alanı "(multi-lang)" işaretlemiyor (Title/Excerpt/Content'in
/// aksine), bu yüzden Translation'a taşınmadı; native, paylaşılan bir etiket havuzu (Blog yazıları
/// arasında tekrar kullanılabilir, aynı isimde ikinci bir Tag satırı oluşmaz).
/// </summary>
public class Tag
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;

    private Tag()
    {
    }

    public Tag(string name)
    {
        Name = name;
    }
}
