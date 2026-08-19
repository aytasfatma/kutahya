using Domain.Enums;

namespace Application.ReferenceProjects;

public class ReferenceProjectTranslationInput
{
    public int LanguageId { get; init; }
    public string? ProjectName { get; init; }
    public string? Description { get; init; }
    public string? SeoUrl { get; init; }
}

public class ReferenceProjectRequestBase
{
    public string? Location { get; init; }
    public ReferenceProjectRegion Region { get; init; }
    public ProductBrand Brand { get; init; }
    public ReferenceProjectType ProjectType { get; init; }
    public string? Architect { get; init; }
    public int? Year { get; init; }
    public int DisplayOrder { get; init; }
    public IReadOnlyList<int> RelatedProductIds { get; init; } = [];
    public IReadOnlyList<ReferenceProjectTranslationInput> Translations { get; init; } = [];
}

public class CreateReferenceProjectRequest : ReferenceProjectRequestBase
{
}

public class UpdateReferenceProjectRequest : ReferenceProjectRequestBase
{
}
