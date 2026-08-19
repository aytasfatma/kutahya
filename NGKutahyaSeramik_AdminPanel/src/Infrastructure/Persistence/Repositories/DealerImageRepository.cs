using Application.Dealers;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class DealerImageRepository : IDealerImageRepository
{
    private readonly AppDbContext _db;
    public DealerImageRepository(AppDbContext db) => _db = db;
    public Task<DealerImage?> GetByIdAsync(int id) => _db.DealerImages.FirstOrDefaultAsync(x => x.Id == id);
    public async Task<IReadOnlyList<DealerImage>> GetByDealerIdAsync(int dealerId) =>
        await _db.DealerImages.Where(x => x.DealerId == dealerId).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).ToListAsync();
    public Task AddAsync(DealerImage image) => _db.DealerImages.AddAsync(image).AsTask();
    public void Remove(DealerImage image) => _db.DealerImages.Remove(image);
    public void RemoveRange(IEnumerable<DealerImage> images) => _db.DealerImages.RemoveRange(images);
}
