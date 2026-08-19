using Application.Blogs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class BlogRepository : IBlogRepository
{
    private readonly AppDbContext _dbContext;

    public BlogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Blog?> GetByIdAsync(int id) =>
        _dbContext.Blogs.FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IReadOnlyList<Blog>> GetAllAsync() =>
        await _dbContext.Blogs.AsNoTracking().ToListAsync();

    public async Task AddAsync(Blog blog) =>
        await _dbContext.Blogs.AddAsync(blog);

    public void Remove(Blog blog) =>
        _dbContext.Blogs.Remove(blog);

    public async Task<IReadOnlyList<Tag>> GetTagsAsync(int blogId) =>
        await _dbContext.BlogTags
            .Where(bt => bt.BlogId == blogId)
            .Select(bt => bt.Tag)
            .ToListAsync();

    public async Task ReplaceTagsAsync(int blogId, IReadOnlyList<int> tagIds)
    {
        var existing = await _dbContext.BlogTags
            .Where(bt => bt.BlogId == blogId)
            .ToListAsync();

        var newIds = tagIds.ToHashSet();
        var existingIds = existing.Select(bt => bt.TagId).ToHashSet();

        var toRemove = existing.Where(bt => !newIds.Contains(bt.TagId));
        _dbContext.BlogTags.RemoveRange(toRemove);

        foreach (var tagId in newIds.Where(id => !existingIds.Contains(id)))
        {
            await _dbContext.BlogTags.AddAsync(new BlogTag(blogId, tagId));
        }
    }

    public async Task<IReadOnlyList<int>> GetRelatedBlogIdsAsync(int blogId) =>
        await _dbContext.BlogRelatedPosts
            .Where(brp => brp.BlogId == blogId)
            .Select(brp => brp.RelatedBlogId)
            .ToListAsync();

    public async Task ReplaceRelatedPostsAsync(int blogId, IReadOnlyList<int> relatedBlogIds)
    {
        var existing = await _dbContext.BlogRelatedPosts
            .Where(brp => brp.BlogId == blogId)
            .ToListAsync();

        var newIds = relatedBlogIds.Where(id => id != blogId).ToHashSet();
        var existingIds = existing.Select(brp => brp.RelatedBlogId).ToHashSet();

        var toRemove = existing.Where(brp => !newIds.Contains(brp.RelatedBlogId));
        _dbContext.BlogRelatedPosts.RemoveRange(toRemove);

        foreach (var relatedBlogId in newIds.Where(id => !existingIds.Contains(id)))
        {
            await _dbContext.BlogRelatedPosts.AddAsync(new BlogRelatedPost(blogId, relatedBlogId));
        }
    }

    public async Task RemoveRelatedPostReferencesAsync(int blogId)
    {
        var references = await _dbContext.BlogRelatedPosts
            .Where(brp => brp.RelatedBlogId == blogId)
            .ToListAsync();

        _dbContext.BlogRelatedPosts.RemoveRange(references);
    }
}
