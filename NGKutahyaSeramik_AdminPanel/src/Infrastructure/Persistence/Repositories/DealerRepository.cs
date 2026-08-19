using Application.Dealers;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class DealerRepository : IDealerRepository
{
    private readonly AppDbContext _dbContext;

    public DealerRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Dealer?> GetByIdAsync(int id) =>
        _dbContext.Dealers.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IReadOnlyList<Dealer>> GetAllAsync() =>
        await _dbContext.Dealers.AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<Dealer>> GetFilteredAsync(DealerQuery filter)
    {
        var query = _dbContext.Dealers.AsNoTracking().AsQueryable();

        if (filter.Category.HasValue)
        {
            query = query.Where(d => d.Category == filter.Category.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var city = filter.City.Trim();
            query = query.Where(d => EF.Functions.Like(d.City, city));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(d => d.IsActive == filter.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            var like = $"%{term}%";
            var normalizedTerm = term.ToLowerInvariant();
            var matchesHeadquartersCategory = "genel merkez".Contains(normalizedTerm) || normalizedTerm.Contains("genel merkez");
            var matchesFactoryCategory = "fabrika".Contains(normalizedTerm) || normalizedTerm.Contains("fabrika");
            var matchesSalesPointCategory = "satış noktası".Contains(normalizedTerm) || normalizedTerm.Contains("satış noktası");
            var matchesUncategorized = "kategorisiz".Contains(normalizedTerm) || normalizedTerm.Contains("kategorisiz");

            query = query.Where(d =>
                EF.Functions.Like(d.Name, like) ||
                EF.Functions.Like(d.City, like) ||
                (d.District != null && EF.Functions.Like(d.District, like)) ||
                (d.Phone != null && EF.Functions.Like(d.Phone, like)) ||
                (d.Address != null && EF.Functions.Like(d.Address, like)) ||
                (matchesHeadquartersCategory && d.Category == DealerCategory.GeneralHeadquarters) ||
                (matchesFactoryCategory && d.Category == DealerCategory.Factory) ||
                (matchesSalesPointCategory && d.Category == DealerCategory.SalesPoint) ||
                (matchesUncategorized && d.Category == null));
        }

        return await query.OrderBy(d => d.City).ThenBy(d => d.Name).ToListAsync();
    }

    public async Task AddAsync(Dealer dealer) =>
        await _dbContext.Dealers.AddAsync(dealer);

    public void Remove(Dealer dealer) =>
        _dbContext.Dealers.Remove(dealer);
}
