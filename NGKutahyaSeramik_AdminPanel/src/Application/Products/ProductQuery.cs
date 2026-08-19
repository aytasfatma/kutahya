namespace Application.Products;

using Domain.Enums;

public class ProductQuery
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    public string Sort { get; init; } = ProductSortOptions.DisplayOrder;
    public ProductStatus? Status { get; init; }
    public int? CategoryId { get; init; }
    public int? CollectionId { get; init; }
    public ProductBrand? Brand { get; init; }
    public string? Surface { get; init; }
    public string? Search { get; init; }
}

public static class ProductSortOptions
{
    public const string DisplayOrder = "displayOrder";
    public const string ProductCode = "productCode";
    public const string Newest = "newest";
    public const string NameAsc = "nameAsc";
}

public class ProductPagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
