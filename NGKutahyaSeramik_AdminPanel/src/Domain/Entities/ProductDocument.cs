namespace Domain.Entities;

/// <summary>
/// Ürün-doküman ilişki tablosu (Madde 36.2 — many-to-many). Product silinirse yalnızca bu ilişki
/// satırı silinir; Document başka ürünlere/koleksiyonlara veya genel seviyede bağlı kalabileceği
/// için fiziksel dosya ve Document kaydı ETKİLENMEZ.
/// </summary>
public class ProductDocument
{
    public int Id { get; private set; }
    public int ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int DocumentId { get; private set; }
    public Document Document { get; private set; } = null!;

    private ProductDocument()
    {
    }

    public ProductDocument(int productId, int documentId)
    {
        ProductId = productId;
        DocumentId = documentId;
    }
}
