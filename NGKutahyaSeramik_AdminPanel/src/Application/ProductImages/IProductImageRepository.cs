using Domain.Entities;

namespace Application.ProductImages;

public interface IProductImageRepository
{
    Task<ProductImage?> GetByIdAsync(int id);

    Task<IReadOnlyList<ProductImage>> GetByProductIdAsync(int productId);

    Task AddAsync(ProductImage image);

    void Remove(ProductImage image);

    void RemoveRange(IEnumerable<ProductImage> images);
}
