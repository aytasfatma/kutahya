using System.ComponentModel.DataAnnotations;
using Application.Seo;
using Domain.Enums;

namespace Presentation.Models.Seo;

public class SeoLanguageFieldViewModel
{
    public int LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;

    [Display(Name = "SEO URL")]
    public string? SeoUrl { get; set; }

    [Display(Name = "Meta Başlık")]
    public string? MetaTitle { get; set; }

    [Display(Name = "Meta Açıklama")]
    public string? MetaDescription { get; set; }
}

public class SeoEditViewModel
{
    public EntityType EntityType { get; set; }
    public int EntityId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool SupportsMetaFields { get; set; }
    public List<SeoLanguageFieldViewModel> Languages { get; set; } = [];
}

public class SeoIndexSummaryViewModel
{
    public int TotalRecords { get; init; }
    public int MissingRows { get; init; }
    public int DuplicateGroups { get; init; }
    public int SeoUrlOnlyRecords { get; init; }
}

public class SeoIndexContentTypeViewModel
{
    public EntityType EntityType { get; init; }
    public string Label { get; init; } = string.Empty;
    public bool SupportsMetaFields { get; init; }
    public bool IsSelected { get; init; }
}

public class SeoIndexRowViewModel
{
    public EntityType EntityType { get; init; }
    public int EntityId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string ContentTypeLabel { get; init; } = string.Empty;
    public int LanguageId { get; init; }
    public string LanguageCode { get; init; } = string.Empty;
    public string LanguageName { get; init; } = string.Empty;
    public string? SeoUrl { get; init; }
    public bool SupportsMetaFields { get; init; }
    public bool MissingSeoUrl { get; init; }
    public bool MissingMetaTitle { get; init; }
    public bool MissingMetaDescription { get; init; }
    public bool IsDuplicateUrl { get; init; }
    public string HealthLabel { get; init; } = string.Empty;
    public string HealthBadgeClass { get; init; } = string.Empty;
}

public class SeoIndexViewModel
{
    public IReadOnlyList<SeoIndexContentTypeViewModel> ContentTypes { get; init; } = [];
    public SeoIndexSummaryViewModel Summary { get; init; } = new();
    public EntityType? SelectedType { get; init; }
    public string? SelectedTypeLabel { get; init; }
    public IReadOnlyList<SeoIndexRowViewModel> Rows { get; init; } = [];
}
