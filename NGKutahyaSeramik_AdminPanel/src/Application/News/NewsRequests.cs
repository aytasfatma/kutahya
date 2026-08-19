using Domain.Enums;

namespace Application.News;

public class NewsTranslationInput
{
    public int LanguageId { get; init; }
    public string? Title { get; init; }
    public string? Excerpt { get; init; }
    public string? Content { get; init; }
    public string? SeoUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}

public class NewsRequestBase
{
    public int? NewsCategoryId { get; init; }
    public DateTime? PublishDate { get; init; }
    public NewsStatus Status { get; init; }
    public IReadOnlyList<int> RelatedNewsIds { get; init; } = [];
    public IReadOnlyList<NewsTranslationInput> Translations { get; init; } = [];

    /// <summary>Kapak görseli tamamen opsiyoneldir (Madde 22'de Zorunluluk bilgisi yok) — Content null ise yükleme yapılmaz.</summary>
    public string? FeaturedImageOriginalFileName { get; init; }
    public string? FeaturedImageContentType { get; init; }
    public long? FeaturedImageLength { get; init; }
    public Stream? FeaturedImageContent { get; init; }
}

public class CreateNewsRequest : NewsRequestBase
{
}

public class UpdateNewsRequest : NewsRequestBase
{
    /// <summary>Yeni görsel yüklenmeden mevcut kapak görselinin kaldırılması istendiğinde true.</summary>
    public bool RemoveFeaturedImage { get; init; }
}
