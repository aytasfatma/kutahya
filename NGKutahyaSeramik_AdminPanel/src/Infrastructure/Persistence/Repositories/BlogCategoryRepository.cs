using Application.Blogs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class BlogCategoryRepository : IBlogCategoryRepository
{
    private readonly AppDbContext _dbContext;

    public BlogCategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<BlogCategory?> GetByIdAsync(int id) =>
        _dbContext.BlogCategories.FirstOrDefaultAsync(bc => bc.Id == id);

    public async Task<IReadOnlyList<BlogCategory>> GetAllAsync() =>
        await _dbContext.BlogCategories.AsNoTracking().ToListAsync();

    public async Task AddAsync(BlogCategory blogCategory) =>
        await _dbContext.BlogCategories.AddAsync(blogCategory);

    public void Remove(BlogCategory blogCategory) =>
        _dbContext.BlogCategories.Remove(blogCategory);
}
