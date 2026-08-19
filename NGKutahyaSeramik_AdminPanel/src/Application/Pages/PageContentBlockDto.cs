using Domain.Enums;

namespace Application.Pages;

public class PageContentBlockTranslationDto
{
    public int LanguageId { get; init; }
    public string LanguageCode { get; init; } = string.Empty;
    public string LanguageName { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? Content { get; init; }
}

public class PageContentBlockDto
{
    public int Id { get; init; }
    public int PageId { get; init; }
    public PageBlockType BlockType { get; init; }
    public int DisplayOrder { get; init; }
    public string? ImagePath { get; init; }
    public string? VideoEmbedUrl { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public IReadOnlyList<PageContentBlockTranslationDto> Translations { get; init; } = [];

    public string BlockTypeLabel => PageEnumDisplay.GetBlockTypeLabel(BlockType);
    public string? DisplayTitle => Translations.FirstOrDefault(t => t.LanguageCode == "TR")?.Title;
}
