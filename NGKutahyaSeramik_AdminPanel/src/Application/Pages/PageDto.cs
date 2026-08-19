namespace Application.Pages;

public class PageTranslationDto
{
    public int LanguageId { get; init; }
    public string LanguageCode { get; init; } = string.Empty;
    public string LanguageName { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? SeoUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}

public class PageDto
{
    public int Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public IReadOnlyList<PageTranslationDto> Translations { get; init; } = [];

    public string? DisplayTitle => Translations.FirstOrDefault(t => t.LanguageCode == "TR")?.Title;
}
