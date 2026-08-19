namespace Application.Banners;

public class BannerTranslationDto
{
    public int LanguageId { get; init; }
    public string LanguageCode { get; init; } = string.Empty;
    public string LanguageName { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? Subtitle { get; init; }
    public string? ButtonText { get; init; }
    public string? ButtonUrl { get; init; }
}

public class BannerDto
{
    public int Id { get; init; }
    public string? ImagePath { get; init; }
    public DateTime? PublishStartDate { get; init; }
    public DateTime? PublishEndDate { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<BannerTranslationDto> Translations { get; init; } = [];

    public string? DisplayName => Translations.FirstOrDefault(t => t.LanguageCode == "TR")?.Title;
}
