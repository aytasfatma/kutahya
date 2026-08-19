using Application.ProductImages;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ProductImageRepository : IProductImageRepository
{
    private readonly AppDbContext _dbContext;

    public ProductImageRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ProductImage?> GetByIdAsync(int id) =>
        _dbContext.ProductImages.FirstOrDefaultAsync(pi => pi.Id == id);

    public async Task<IReadOnlyList<ProductImage>> GetByProductIdAsync(int productId) =>
        await _dbContext.ProductImages
            .Where(pi => pi.ProductId == productId)
            .OrderBy(pi => pi.DisplayOrder)
            .ToListAsync();

    public async Task AddAsync(ProductImage image) =>
        await _dbContext.ProductImages.AddAsync(image);

    public void Remove(ProductImage image) =>
        _dbContext.ProductImages.Remove(image);

    public void RemoveRange(IEnumerable<ProductImage> images) =>
        _dbContext.ProductImages.RemoveRange(images);
}
