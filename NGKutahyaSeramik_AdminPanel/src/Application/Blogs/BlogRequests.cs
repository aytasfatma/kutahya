using Domain.Enums;

namespace Application.Blogs;

public class BlogTranslationInput
{
    public int LanguageId { get; init; }
    public string? Title { get; init; }
    public string? Excerpt { get; init; }
    public string? Content { get; init; }
    public string? SeoUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}

public class BlogRequestBase
{
    public int? BlogCategoryId { get; init; }
    public string? Author { get; init; }
    public DateTime? PublishDate { get; init; }
    public BlogStatus Status { get; init; }
    public bool IsTrend { get; init; }
    public IReadOnlyList<string> TagNames { get; init; } = [];
    public IReadOnlyList<int> RelatedBlogIds { get; init; } = [];
    public IReadOnlyList<BlogTranslationInput> Translations { get; init; } = [];

    /// <summary>Kapak görseli tamamen opsiyoneldir (Madde 21.1'de Zorunluluk sütunu yok) — Content null ise yükleme yapılmaz.</summary>
    public string? FeaturedImageOriginalFileName { get; init; }
    public string? FeaturedImageContentType { get; init; }
    public long? FeaturedImageLength { get; init; }
    public Stream? FeaturedImageContent { get; init; }
    public string? SecondaryImageOriginalFileName { get; init; }
    public string? SecondaryImageContentType { get; init; }
    public long? SecondaryImageLength { get; init; }
    public Stream? SecondaryImageContent { get; init; }
}

public class CreateBlogRequest : BlogRequestBase
{
}

public class UpdateBlogRequest : BlogRequestBase
{
    /// <summary>Yeni görsel yüklenmeden mevcut kapak görselinin kaldırılması istendiğinde true.</summary>
    public bool RemoveFeaturedImage { get; init; }
    public bool RemoveSecondaryImage { get; init; }
}
