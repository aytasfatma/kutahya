using Domain.Enums;

namespace Application.Categories;

public class CategoryTranslationDto
{
    public int LanguageId { get; init; }
    public string LanguageCode { get; init; } = string.Empty;
    public string LanguageName { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? SeoUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}

public class CategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? SeoUrl { get; init; }
    public int? ParentCategoryId { get; init; }
    public string? ImagePath { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public int ProductCount { get; init; }
    public IReadOnlyList<ProductBrand> Brands { get; init; } = Array.Empty<ProductBrand>();
    public IReadOnlyList<CategoryTranslationDto> Translations { get; init; } = Array.Empty<CategoryTranslationDto>();

    public string? DisplayName => Name;
}

public sealed class CategoryOptionDto
{
    public int Id { get; init; }
    public int? ParentCategoryId { get; init; }
    public int DisplayOrder { get; init; }
    public string? DisplayName { get; init; }
}
