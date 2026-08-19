using Domain.Entities;

namespace Application.Surfaces;

public interface ISurfaceRepository
{
    Task<IReadOnlyList<Surface>> GetAllAsync();
    Task<Surface?> GetByIdAsync(int id);
    Task<Surface?> GetByNameAsync(string name);
    Task<bool> IsNameInUseAsync(string name, int? excludeId = null);
    Task<bool> HasProductsAsync(int id);
    Task AddAsync(Surface surface);
    void Remove(Surface surface);
}
