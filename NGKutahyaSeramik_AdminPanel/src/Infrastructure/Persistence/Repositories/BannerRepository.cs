using Application.Banners;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class BannerRepository : IBannerRepository
{
    private readonly AppDbContext _dbContext;

    public BannerRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Banner?> GetByIdAsync(int id) =>
        _dbContext.Banners.FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IReadOnlyList<Banner>> GetAllAsync() =>
        await _dbContext.Banners.AsNoTracking().ToListAsync();

    public async Task AddAsync(Banner banner) =>
        await _dbContext.Banners.AddAsync(banner);

    public void Remove(Banner banner) =>
        _dbContext.Banners.Remove(banner);
}
