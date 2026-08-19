using Application.News;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class NewsCategoryRepository : INewsCategoryRepository
{
    private readonly AppDbContext _dbContext;

    public NewsCategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<NewsCategory?> GetByIdAsync(int id) =>
        _dbContext.NewsCategories.FirstOrDefaultAsync(nc => nc.Id == id);

    public async Task<IReadOnlyList<NewsCategory>> GetAllAsync() =>
        await _dbContext.NewsCategories.AsNoTracking().ToListAsync();

    public async Task AddAsync(NewsCategory newsCategory) =>
        await _dbContext.NewsCategories.AddAsync(newsCategory);

    public void Remove(NewsCategory newsCategory) =>
        _dbContext.NewsCategories.Remove(newsCategory);
}
