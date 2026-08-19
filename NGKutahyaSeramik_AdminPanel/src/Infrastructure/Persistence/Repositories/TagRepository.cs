using Application.Blogs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TagRepository : ITagRepository
{
    private readonly AppDbContext _dbContext;

    public TagRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Tag?> GetByNameAsync(string name) =>
        _dbContext.Tags.FirstOrDefaultAsync(t => t.Name == name);

    public async Task<IReadOnlyList<Tag>> GetAllAsync() =>
        await _dbContext.Tags.AsNoTracking().ToListAsync();

    public async Task AddAsync(Tag tag) =>
        await _dbContext.Tags.AddAsync(tag);
}
