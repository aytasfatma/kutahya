using Application.Documents;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _dbContext;

    public DocumentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Document?> GetByIdAsync(int id) =>
        _dbContext.Documents.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IReadOnlyList<Document>> GetAllAsync() =>
        await _dbContext.Documents.AsNoTracking().ToListAsync();

    public async Task AddAsync(Document document) =>
        await _dbContext.Documents.AddAsync(document);

    public void Remove(Document document) =>
        _dbContext.Documents.Remove(document);

    public async Task<IReadOnlyList<int>> GetRelatedProductIdsAsync(int documentId) =>
        await _dbContext.ProductDocuments
            .Where(pd => pd.DocumentId == documentId)
            .Select(pd => pd.ProductId)
            .ToListAsync();

    public async Task<IReadOnlyList<int>> GetRelatedCollectionIdsAsync(int documentId) =>
        await _dbContext.CollectionDocuments
            .Where(cd => cd.DocumentId == documentId)
            .Select(cd => cd.CollectionId)
            .ToListAsync();

    public async Task ReplaceProductRelationsAsync(int documentId, IReadOnlyList<int> productIds)
    {
        var existing = await _dbContext.ProductDocuments
            .Where(pd => pd.DocumentId == documentId)
            .ToListAsync();

        var newIds = productIds.ToHashSet();
        var existingIds = existing.Select(pd => pd.ProductId).ToHashSet();

        var toRemove = existing.Where(pd => !newIds.Contains(pd.ProductId));
        _dbContext.ProductDocuments.RemoveRange(toRemove);

        foreach (var productId in newIds.Where(id => !existingIds.Contains(id)))
        {
            await _dbContext.ProductDocuments.AddAsync(new ProductDocument(productId, documentId));
        }
    }

    public async Task ReplaceCollectionRelationsAsync(int documentId, IReadOnlyList<int> collectionIds)
    {
        var existing = await _dbContext.CollectionDocuments
            .Where(cd => cd.DocumentId == documentId)
            .ToListAsync();

        var newIds = collectionIds.ToHashSet();
        var existingIds = existing.Select(cd => cd.CollectionId).ToHashSet();

        var toRemove = existing.Where(cd => !newIds.Contains(cd.CollectionId));
        _dbContext.CollectionDocuments.RemoveRange(toRemove);

        foreach (var collectionId in newIds.Where(id => !existingIds.Contains(id)))
        {
            await _dbContext.CollectionDocuments.AddAsync(new CollectionDocument(collectionId, documentId));
        }
    }
}
