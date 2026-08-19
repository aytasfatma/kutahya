using Domain.Enums;

namespace Application.Collections;

public class CollectionTranslationInput
{
    public int LanguageId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? SeoUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}

public class CreateCollectionRequest
{
    public string? ImagePath { get; init; }
    public int DisplayOrder { get; init; }
    public IReadOnlyList<ProductBrand> Brands { get; init; } = Enum.GetValues<ProductBrand>();
    public IReadOnlyList<CollectionTranslationInput> Translations { get; init; } = Array.Empty<CollectionTranslationInput>();
}

public class UpdateCollectionRequest
{
    public string? ImagePath { get; init; }
    public int DisplayOrder { get; init; }
    public IReadOnlyList<ProductBrand> Brands { get; init; } = Enum.GetValues<ProductBrand>();
    public IReadOnlyList<CollectionTranslationInput> Translations { get; init; } = Array.Empty<CollectionTranslationInput>();
}
