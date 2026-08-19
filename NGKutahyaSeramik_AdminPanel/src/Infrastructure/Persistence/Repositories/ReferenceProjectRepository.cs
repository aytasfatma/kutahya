using Application.ReferenceProjects;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ReferenceProjectRepository : IReferenceProjectRepository
{
    private readonly AppDbContext _dbContext;

    public ReferenceProjectRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ReferenceProject?> GetByIdAsync(int id) =>
        _dbContext.ReferenceProjects.FirstOrDefaultAsync(rp => rp.Id == id);

    public async Task<IReadOnlyList<ReferenceProject>> GetAllAsync() =>
        await _dbContext.ReferenceProjects.AsNoTracking().ToListAsync();

    public async Task AddAsync(ReferenceProject referenceProject) =>
        await _dbContext.ReferenceProjects.AddAsync(referenceProject);

    public void Remove(ReferenceProject referenceProject) =>
        _dbContext.ReferenceProjects.Remove(referenceProject);

    public async Task<IReadOnlyList<int>> GetRelatedProductIdsAsync(int referenceProjectId) =>
        await _dbContext.ProductReferenceProjects
            .Where(prp => prp.ReferenceProjectId == referenceProjectId)
            .Select(prp => prp.ProductId)
            .ToListAsync();

    public async Task ReplaceProductRelationsAsync(int referenceProjectId, IReadOnlyList<int> productIds)
    {
        var existing = await _dbContext.ProductReferenceProjects
            .Where(prp => prp.ReferenceProjectId == referenceProjectId)
            .ToListAsync();

        var newIds = productIds.ToHashSet();
        var existingIds = existing.Select(prp => prp.ProductId).ToHashSet();

        var toRemove = existing.Where(prp => !newIds.Contains(prp.ProductId));
        _dbContext.ProductReferenceProjects.RemoveRange(toRemove);

        foreach (var productId in newIds.Where(id => !existingIds.Contains(id)))
        {
            await _dbContext.ProductReferenceProjects.AddAsync(new ProductReferenceProject(productId, referenceProjectId));
        }
    }
}
