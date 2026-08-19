using Domain.Enums;

namespace Application.ReferenceProjects;

public class ReferenceProjectTranslationDto
{
    public int LanguageId { get; init; }
    public string LanguageCode { get; init; } = string.Empty;
    public string LanguageName { get; init; } = string.Empty;
    public string? ProjectName { get; init; }
    public string? Description { get; init; }
    public string? SeoUrl { get; init; }
}

public class ReferenceProjectRelatedProductDto
{
    public int Id { get; init; }
    public string Label { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string SeoUrl { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
}

public class ReferenceProjectDto
{
    public int Id { get; init; }
    public string? Location { get; init; }
    public ReferenceProjectRegion Region { get; init; }
    public ProductBrand Brand { get; init; }
    public ReferenceProjectType ProjectType { get; init; }
    public string? Architect { get; init; }
    public int? Year { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public string? FeaturedImagePath { get; init; }
    public IReadOnlyList<ReferenceProjectTranslationDto> Translations { get; init; } = [];
    public IReadOnlyList<ReferenceProjectRelatedProductDto> RelatedProducts { get; init; } = [];

    public string? DisplayName => Translations.FirstOrDefault(t => t.LanguageCode == "TR")?.ProjectName;
    public string RegionLabel => ReferenceProjectEnumDisplay.GetRegionLabel(Region);
    public string BrandLabel => ReferenceProjectEnumDisplay.GetBrandLabel(Brand);
    public string ProjectTypeLabel => ReferenceProjectEnumDisplay.GetProjectTypeLabel(ProjectType);
}
