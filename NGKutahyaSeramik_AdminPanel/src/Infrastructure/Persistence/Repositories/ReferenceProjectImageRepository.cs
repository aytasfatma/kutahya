using Application.ReferenceProjects;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ReferenceProjectImageRepository : IReferenceProjectImageRepository
{
    private readonly AppDbContext _dbContext;

    public ReferenceProjectImageRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ReferenceProjectImage?> GetByIdAsync(int id) =>
        _dbContext.ReferenceProjectImages.FirstOrDefaultAsync(i => i.Id == id);

    public async Task<IReadOnlyList<ReferenceProjectImage>> GetByReferenceProjectIdAsync(int referenceProjectId) =>
        await _dbContext.ReferenceProjectImages
            .Where(i => i.ReferenceProjectId == referenceProjectId)
            .ToListAsync();

    public async Task AddAsync(ReferenceProjectImage image) =>
        await _dbContext.ReferenceProjectImages.AddAsync(image);

    public void Remove(ReferenceProjectImage image) =>
        _dbContext.ReferenceProjectImages.Remove(image);

    public void RemoveRange(IEnumerable<ReferenceProjectImage> images) =>
        _dbContext.ReferenceProjectImages.RemoveRange(images);
}
