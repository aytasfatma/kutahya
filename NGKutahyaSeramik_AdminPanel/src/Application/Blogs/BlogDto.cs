using Domain.Enums;

namespace Application.Blogs;

public class BlogTranslationDto
{
    public int LanguageId { get; init; }
    public string LanguageCode { get; init; } = string.Empty;
    public string LanguageName { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? Excerpt { get; init; }
    public string? Content { get; init; }
    public string? SeoUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}

public class BlogRelatedPostSummaryDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? SeoUrl { get; init; }
    public string? FeaturedImagePath { get; init; }
}

public class BlogDto
{
    public int Id { get; init; }
    public int? BlogCategoryId { get; init; }
    public string? BlogCategoryName { get; init; }
    public string? Author { get; init; }
    public DateTime? PublishDate { get; init; }
    public BlogStatus Status { get; init; }
    public bool IsTrend { get; init; }
    public string? FeaturedImagePath { get; init; }
    public string? SecondaryImagePath { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public IReadOnlyList<BlogRelatedPostSummaryDto> RelatedPosts { get; init; } = [];
    public IReadOnlyList<BlogTranslationDto> Translations { get; init; } = [];

    public string? DisplayTitle => Translations.FirstOrDefault(t => t.LanguageCode == "TR")?.Title;
    public string StatusLabel => BlogEnumDisplay.GetStatusLabel(Status);
}
