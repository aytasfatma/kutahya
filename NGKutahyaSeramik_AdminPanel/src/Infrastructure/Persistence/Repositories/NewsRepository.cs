using Application.News;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class NewsRepository : INewsRepository
{
    private readonly AppDbContext _dbContext;

    public NewsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<News?> GetByIdAsync(int id) =>
        _dbContext.News.FirstOrDefaultAsync(n => n.Id == id);

    public async Task<IReadOnlyList<News>> GetAllAsync() =>
        await _dbContext.News.AsNoTracking().ToListAsync();

    public async Task AddAsync(News news) =>
        await _dbContext.News.AddAsync(news);

    public void Remove(News news) =>
        _dbContext.News.Remove(news);

    public async Task<IReadOnlyList<int>> GetRelatedNewsIdsAsync(int newsId) =>
        await _dbContext.NewsRelatedPosts
            .Where(nrp => nrp.NewsId == newsId)
            .Select(nrp => nrp.RelatedNewsId)
            .ToListAsync();

    public async Task ReplaceRelatedNewsAsync(int newsId, IReadOnlyList<int> relatedNewsIds)
    {
        var existing = await _dbContext.NewsRelatedPosts
            .Where(nrp => nrp.NewsId == newsId)
            .ToListAsync();

        var newIds = relatedNewsIds.Where(id => id != newsId).ToHashSet();
        var existingIds = existing.Select(nrp => nrp.RelatedNewsId).ToHashSet();

        var toRemove = existing.Where(nrp => !newIds.Contains(nrp.RelatedNewsId));
        _dbContext.NewsRelatedPosts.RemoveRange(toRemove);

        foreach (var relatedNewsId in newIds.Where(id => !existingIds.Contains(id)))
        {
            await _dbContext.NewsRelatedPosts.AddAsync(new NewsRelatedPost(newsId, relatedNewsId));
        }
    }

    public async Task RemoveRelatedPostReferencesAsync(int newsId)
    {
        var references = await _dbContext.NewsRelatedPosts
            .Where(nrp => nrp.RelatedNewsId == newsId)
            .ToListAsync();

        _dbContext.NewsRelatedPosts.RemoveRange(references);
    }
}
