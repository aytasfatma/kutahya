using Domain.Entities;

namespace Application.ReferenceProjects;

public interface IReferenceProjectImageRepository
{
    Task<ReferenceProjectImage?> GetByIdAsync(int id);

    Task<IReadOnlyList<ReferenceProjectImage>> GetByReferenceProjectIdAsync(int referenceProjectId);

    Task AddAsync(ReferenceProjectImage image);

    void Remove(ReferenceProjectImage image);

    void RemoveRange(IEnumerable<ReferenceProjectImage> images);
}
