using Domain.Enums;

namespace Application.News;

public class NewsTranslationDto
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

public class NewsRelatedPostSummaryDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? SeoUrl { get; init; }
    public string? FeaturedImagePath { get; init; }
}

public class NewsDto
{
    public int Id { get; init; }
    public int? NewsCategoryId { get; init; }
    public string? NewsCategoryName { get; init; }
    public DateTime? PublishDate { get; init; }
    public NewsStatus Status { get; init; }
    public string? FeaturedImagePath { get; init; }
    public IReadOnlyList<NewsRelatedPostSummaryDto> RelatedPosts { get; init; } = [];
    public IReadOnlyList<NewsTranslationDto> Translations { get; init; } = [];

    public string? DisplayTitle => Translations.FirstOrDefault(t => t.LanguageCode == "TR")?.Title;
    public string StatusLabel => NewsEnumDisplay.GetStatusLabel(Status);
}
