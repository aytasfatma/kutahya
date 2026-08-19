using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Madde 16.2'nin 5 blok tipini (metin+görsel/tam-genişlik-görsel/video-embed/akordeon/tab) tek,
/// düz bir entity'de birleştirir — her satır bağımsız bir içerik birimidir. Accordion/Tab'ın çoklu
/// panel/sekme grup yapısı dokümanda tanımlanmadığı için ayrı bir grup kimliği/alt tablo icat
/// edilmedi (MVP sınırlaması, kapanış raporunda kayıtlı). Title/Content Translation'da
/// (EntityType.PageContentBlock); ImagePath/VideoEmbedUrl native — blok tipine göre hangisinin
/// kullanılacağı Application katmanında (PageContentBlockService) doğrulanır.
/// </summary>
public class PageContentBlock
{
    public int Id { get; private set; }
    public int PageId { get; private set; }
    public Page Page { get; private set; } = null!;
    public PageBlockType BlockType { get; private set; }
    public int DisplayOrder { get; private set; }
    public string? ImagePath { get; private set; }
    public string? VideoEmbedUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private PageContentBlock()
    {
    }

    public PageContentBlock(int pageId, PageBlockType blockType, int displayOrder, string? videoEmbedUrl, bool isActive = true)
    {
        PageId = pageId;
        BlockType = blockType;
        DisplayOrder = displayOrder;
        VideoEmbedUrl = videoEmbedUrl;
        IsActive = isActive;

        var now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public void UpdateBlockType(PageBlockType blockType, string? videoEmbedUrl)
    {
        BlockType = blockType;
        VideoEmbedUrl = videoEmbedUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetImagePath(string? filePath)
    {
        ImagePath = filePath;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDisplayOrder(int displayOrder) => DisplayOrder = displayOrder;
}
