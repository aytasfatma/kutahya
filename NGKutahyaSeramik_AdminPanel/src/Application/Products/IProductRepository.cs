using Domain.Entities;

namespace Application.Products;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);

    Task<Product?> GetBySeoUrlAsync(string seoUrl, string languageCode);

    Task<IReadOnlyList<Product>> GetAllAsync();

    Task<(IReadOnlyList<Product> Items, int TotalCount, int PageNumber)> GetPagedAsync(ProductQuery query);

    Task<ProductPagedResult<ProductListItemDto>> GetPagedListAsync(ProductQuery query);

    Task<ProductTechnicalOptionsDto> GetTechnicalOptionsAsync();

    Task<int> GetNextDisplayOrderAsync();

    Task<IReadOnlyList<(int Id, string Label)>> GetProductCodeOptionsAsync();

    Task<Product?> GetByProductCodeAsync(string productCode);

    Task<bool> HasAnyWithCategoryIdAsync(int categoryId);

    Task<int> CountByCategoryIdAsync(int categoryId);

    Task<bool> HasAnyWithCollectionIdAsync(int collectionId);

    Task AddAsync(Product product);

    void Remove(Product product);
}
