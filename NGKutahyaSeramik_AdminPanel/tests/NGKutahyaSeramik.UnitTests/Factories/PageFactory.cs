using Domain.Entities;
using Domain.Enums;

namespace NGKutahyaSeramik.UnitTests.Factories;

public static class PageFactory
{
    public static Page CreateValid() => new();
}

public static class PageContentBlockFactory
{
    public static PageContentBlock CreateTextImageBlock(int pageId, int displayOrder = 0) =>
        new(pageId, PageBlockType.TextImage, displayOrder, videoEmbedUrl: null);

    public static PageContentBlock CreateFullWidthImageBlock(int pageId, int displayOrder = 0) =>
        new(pageId, PageBlockType.FullWidthImage, displayOrder, videoEmbedUrl: null);

    public static PageContentBlock CreateVideoBlock(int pageId, string videoEmbedUrl = "https://youtube.com/embed/test", int displayOrder = 0) =>
        new(pageId, PageBlockType.VideoEmbed, displayOrder, videoEmbedUrl);

    public static PageContentBlock CreateAccordionBlock(int pageId, int displayOrder = 0) =>
        new(pageId, PageBlockType.Accordion, displayOrder, videoEmbedUrl: null);

    public static PageContentBlock CreateTabBlock(int pageId, int displayOrder = 0) =>
        new(pageId, PageBlockType.Tab, displayOrder, videoEmbedUrl: null);
}
