using Application.Pages;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class PageRepository : IPageRepository
{
    private readonly AppDbContext _dbContext;

    public PageRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Page?> GetByIdAsync(int id) =>
        _dbContext.Pages.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IReadOnlyList<Page>> GetAllAsync() =>
        await _dbContext.Pages.AsNoTracking().ToListAsync();

    public async Task AddAsync(Page page) =>
        await _dbContext.Pages.AddAsync(page);

    public void Remove(Page page) =>
        _dbContext.Pages.Remove(page);
}
