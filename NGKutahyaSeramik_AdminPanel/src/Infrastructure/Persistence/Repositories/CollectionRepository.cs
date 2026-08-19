using Application.Collections;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CollectionRepository : ICollectionRepository
{
    private const string TrLanguageCode = "TR";

    private readonly AppDbContext _dbContext;

    public CollectionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Collection?> GetByIdAsync(int id) =>
        _dbContext.Collections.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IReadOnlyList<Collection>> GetAllAsync() =>
        await _dbContext.Collections.AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<CollectionOptionDto>> GetOptionItemsAsync() =>
        await _dbContext.Collections
            .AsNoTracking()
            .Select(c => new CollectionOptionDto
            {
                Id = c.Id,
                DisplayOrder = c.DisplayOrder,
                DisplayName = c.Name
            })
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.DisplayName)
            .ThenBy(c => c.Id)
            .ToListAsync();

    public async Task AddAsync(Collection collection) =>
        await _dbContext.Collections.AddAsync(collection);

    public void Remove(Collection collection) =>
        _dbContext.Collections.Remove(collection);
}
