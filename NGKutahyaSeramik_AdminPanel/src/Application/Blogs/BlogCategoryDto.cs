namespace Application.Blogs;

public class BlogCategoryTranslationDto
{
    public int LanguageId { get; init; }
    public string LanguageCode { get; init; } = string.Empty;
    public string LanguageName { get; init; } = string.Empty;
    public string? Name { get; init; }
}

public class BlogCategoryDto
{
    public int Id { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<BlogCategoryTranslationDto> Translations { get; init; } = [];

    public string? DisplayName => Translations.FirstOrDefault(t => t.LanguageCode == "TR")?.Name;
}
