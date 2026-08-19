namespace Presentation.Models.Api;

public sealed record PublicBlogListResponse(
    int Id, string Title, string? Excerpt, string? SeoUrl, string? CategoryName,
    string? Author, DateTime? PublishDate, string? FeaturedImageUrl, IReadOnlyList<string> Tags, bool IsTrend);

public sealed record PublicRelatedPostResponse(int Id, string Title, string? SeoUrl, string? FeaturedImageUrl);

public sealed class PublicBlogDetailResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Excerpt { get; init; }
    public string? Content { get; init; }
    public string? SeoUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? CategoryName { get; init; }
    public string? Author { get; init; }
    public DateTime? PublishDate { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public string? SecondaryImageUrl { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public bool IsTrend { get; init; }
    public IReadOnlyList<PublicRelatedPostResponse> RelatedPosts { get; init; } = [];
}

public sealed record PublicNewsListResponse(
    int Id, string Title, string? SeoUrl, string? CategoryName,
    DateTime? PublishDate, string? FeaturedImageUrl);

public sealed class PublicNewsDetailResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Excerpt { get; init; }
    public string? Content { get; init; }
    public string? SeoUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? CategoryName { get; init; }
    public DateTime? PublishDate { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public IReadOnlyList<PublicRelatedPostResponse> RelatedPosts { get; init; } = [];
}

public sealed record PublicBannerResponse(
    int Id, string? Title, string? Subtitle, string? ButtonText, string? ButtonUrl,
    string? ImageUrl, DateTime? PublishStartDate, DateTime? PublishEndDate, int DisplayOrder);

public sealed record PublicPageListResponse(
    int Id, string Title, string? SeoUrl, string? MetaTitle, string? MetaDescription, DateTime UpdatedAt);

public sealed record PublicPageBlockResponse(
    int Id, string BlockType, string BlockTypeLabel, string? Title, string? Content,
    string? ImageUrl, string? VideoEmbedUrl, int DisplayOrder);

public sealed class PublicPageDetailResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? SeoUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public DateTime UpdatedAt { get; init; }
    public IReadOnlyList<PublicPageBlockResponse> Blocks { get; init; } = [];
}
