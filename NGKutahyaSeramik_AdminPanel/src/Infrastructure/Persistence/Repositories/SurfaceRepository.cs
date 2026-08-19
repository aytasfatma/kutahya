using Application.Surfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SurfaceRepository : ISurfaceRepository
{
    private readonly AppDbContext _db;
    public SurfaceRepository(AppDbContext db) => _db = db;
    public async Task<IReadOnlyList<Surface>> GetAllAsync() => await _db.Surfaces.AsNoTracking().OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToListAsync();
    public Task<Surface?> GetByIdAsync(int id) => _db.Surfaces.FirstOrDefaultAsync(x => x.Id == id);
    public Task<Surface?> GetByNameAsync(string name) => _db.Surfaces.FirstOrDefaultAsync(x => x.Name == name);
    public Task<bool> IsNameInUseAsync(string name, int? excludeId = null) => _db.Surfaces.AnyAsync(x => x.Name == name && (!excludeId.HasValue || x.Id != excludeId));
    public Task<bool> HasProductsAsync(int id) => _db.Products.AnyAsync(x => x.SurfaceId == id);
    public async Task AddAsync(Surface surface) => await _db.Surfaces.AddAsync(surface);
    public void Remove(Surface surface) => _db.Surfaces.Remove(surface);
}
