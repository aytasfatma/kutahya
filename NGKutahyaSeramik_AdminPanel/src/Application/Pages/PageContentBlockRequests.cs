using Domain.Enums;

namespace Application.Pages;

public class PageContentBlockTranslationInput
{
    public int LanguageId { get; init; }
    public string? Title { get; init; }
    public string? Content { get; init; }
}

public class PageContentBlockRequestBase
{
    public PageBlockType BlockType { get; init; }
    public bool IsActive { get; init; } = true;
    public bool EnforceExclusiveActivation { get; init; }
    public string? VideoEmbedUrl { get; init; }
    public IReadOnlyList<PageContentBlockTranslationInput> Translations { get; init; } = [];

    /// <summary>Yalnızca TextImage/FullWidthImage blok tiplerinde kullanılır — VideoEmbed/Accordion/Tab için görsel yüklenemez.</summary>
    public string? ImageOriginalFileName { get; init; }
    public string? ImageContentType { get; init; }
    public long? ImageLength { get; init; }
    public Stream? ImageContent { get; init; }
}

public class AddPageContentBlockRequest : PageContentBlockRequestBase
{
    public int PageId { get; init; }
}

public class UpdatePageContentBlockRequest : PageContentBlockRequestBase
{
    /// <summary>Yeni görsel yüklenmeden mevcut görselin kaldırılması istendiğinde true.</summary>
    public bool RemoveImage { get; init; }
}
