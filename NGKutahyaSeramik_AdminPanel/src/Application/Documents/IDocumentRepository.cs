using Domain.Entities;

namespace Application.Documents;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(int id);

    Task<IReadOnlyList<Document>> GetAllAsync();

    Task AddAsync(Document document);

    void Remove(Document document);

    Task<IReadOnlyList<int>> GetRelatedProductIdsAsync(int documentId);

    Task<IReadOnlyList<int>> GetRelatedCollectionIdsAsync(int documentId);

    /// <summary>Verilen ürün id listesini dokümanın ilişkileriyle birebir eşleşecek şekilde değiştirir (ekleme+silme).</summary>
    Task ReplaceProductRelationsAsync(int documentId, IReadOnlyList<int> productIds);

    /// <summary>Verilen koleksiyon id listesini dokümanın ilişkileriyle birebir eşleşecek şekilde değiştirir.</summary>
    Task ReplaceCollectionRelationsAsync(int documentId, IReadOnlyList<int> collectionIds);
}
