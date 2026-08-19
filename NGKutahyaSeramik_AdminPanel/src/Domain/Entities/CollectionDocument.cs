namespace Domain.Entities;

/// <summary>
/// Koleksiyon-doküman ilişki tablosu (Madde 36.2 — many-to-many). Collection silinirse yalnızca bu
/// ilişki satırı silinir; Document kaydı ve fiziksel dosya ETKİLENMEZ (bkz. ProductDocument).
/// </summary>
public class CollectionDocument
{
    public int Id { get; private set; }
    public int CollectionId { get; private set; }
    public Collection Collection { get; private set; } = null!;
    public int DocumentId { get; private set; }
    public Document Document { get; private set; } = null!;

    private CollectionDocument()
    {
    }

    public CollectionDocument(int collectionId, int documentId)
    {
        CollectionId = collectionId;
        DocumentId = documentId;
    }
}
