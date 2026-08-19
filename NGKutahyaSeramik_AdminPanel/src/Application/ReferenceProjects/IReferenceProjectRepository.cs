using Domain.Entities;

namespace Application.ReferenceProjects;

public interface IReferenceProjectRepository
{
    Task<ReferenceProject?> GetByIdAsync(int id);

    Task<IReadOnlyList<ReferenceProject>> GetAllAsync();

    Task AddAsync(ReferenceProject referenceProject);

    void Remove(ReferenceProject referenceProject);

    Task<IReadOnlyList<int>> GetRelatedProductIdsAsync(int referenceProjectId);

    /// <summary>Verilen ürün id listesini projenin ilişkileriyle birebir eşleşecek şekilde değiştirir (ekleme+silme).</summary>
    Task ReplaceProductRelationsAsync(int referenceProjectId, IReadOnlyList<int> productIds);
}
