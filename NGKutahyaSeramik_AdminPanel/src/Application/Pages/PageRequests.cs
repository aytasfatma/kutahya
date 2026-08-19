namespace Application.Pages;

public class PageTranslationInput
{
    public int LanguageId { get; init; }
    public string? Title { get; init; }
    public string? SeoUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}

public class CreatePageRequest
{
    public IReadOnlyList<PageTranslationInput> Translations { get; init; } = [];
}

public class UpdatePageRequest
{
    public IReadOnlyList<PageTranslationInput> Translations { get; init; } = [];
}
