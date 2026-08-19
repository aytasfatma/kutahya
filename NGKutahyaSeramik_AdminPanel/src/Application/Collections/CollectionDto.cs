using Domain.Enums;

namespace Application.Collections;

public class CollectionTranslationDto
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

public class CollectionDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? SeoUrl { get; init; }
    public string? ImagePath { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public int ProductCount { get; init; }
    public IReadOnlyList<ProductBrand> Brands { get; init; } = Array.Empty<ProductBrand>();
    public IReadOnlyList<CollectionTranslationDto> Translations { get; init; } = Array.Empty<CollectionTranslationDto>();

    public string? DisplayName => Name;
}

public sealed class CollectionOptionDto
{
    public int Id { get; init; }
    public int DisplayOrder { get; init; }
    public string? DisplayName { get; init; }
}
