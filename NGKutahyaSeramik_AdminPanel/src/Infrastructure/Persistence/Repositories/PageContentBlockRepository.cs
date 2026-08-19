using Application.Pages;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class PageContentBlockRepository : IPageContentBlockRepository
{
    private readonly AppDbContext _dbContext;

    public PageContentBlockRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PageContentBlock?> GetByIdAsync(int id) =>
        _dbContext.PageContentBlocks.FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IReadOnlyList<PageContentBlock>> GetByPageIdAsync(int pageId) =>
        await _dbContext.PageContentBlocks
            .Where(b => b.PageId == pageId)
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync();

    public async Task AddAsync(PageContentBlock block) =>
        await _dbContext.PageContentBlocks.AddAsync(block);

    public void Remove(PageContentBlock block) =>
        _dbContext.PageContentBlocks.Remove(block);

    public void RemoveRange(IEnumerable<PageContentBlock> blocks) =>
        _dbContext.PageContentBlocks.RemoveRange(blocks);
}
