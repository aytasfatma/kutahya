using Domain.Enums;

namespace Application.Pages;

public static class PageEnumDisplay
{
    public static string GetBlockTypeLabel(PageBlockType type) => type switch
    {
        PageBlockType.TextImage => "Metin + Görsel",
        PageBlockType.FullWidthImage => "Tam Genişlik Görsel",
        PageBlockType.VideoEmbed => "Video Embed",
        PageBlockType.Accordion => "Akordeon",
        PageBlockType.Tab => "Sekme (Tab)",
        _ => type.ToString()
    };
}
