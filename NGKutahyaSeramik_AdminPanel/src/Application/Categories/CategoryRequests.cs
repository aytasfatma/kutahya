using Domain.Enums;

namespace Application.Categories;

public class CategoryTranslationInput
{
    public int LanguageId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? SeoUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}

public class CreateCategoryRequest
{
    public int? ParentCategoryId { get; init; }
    public string? ImagePath { get; init; }
    public int DisplayOrder { get; init; }
    public IReadOnlyList<ProductBrand> Brands { get; init; } =
        [ProductBrand.NgSeramik, ProductBrand.NgStone, ProductBrand.NgSlim];
    public IReadOnlyList<CategoryTranslationInput> Translations { get; init; } = Array.Empty<CategoryTranslationInput>();
}

public class UpdateCategoryRequest
{
    public int? ParentCategoryId { get; init; }
    public string? ImagePath { get; init; }
    public int DisplayOrder { get; init; }
    public IReadOnlyList<ProductBrand> Brands { get; init; } =
        [ProductBrand.NgSeramik, ProductBrand.NgStone, ProductBrand.NgSlim];
    public IReadOnlyList<CategoryTranslationInput> Translations { get; init; } = Array.Empty<CategoryTranslationInput>();
}
