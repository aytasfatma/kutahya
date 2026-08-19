namespace Domain.Enums;

/// <summary>Madde 22 — "Haber veri modeli blog modülü ile benzer yapıda olacaktır... durum".
/// Blog.Status ile aynı şekil (Draft/Published/Archived), ama proje genelinde enum paylaşımı
/// yapılmadığı için (ProductStatus/BlogStatus/DocumentType hepsi ayrı) bilinçli olarak ayrı bir enum.</summary>
public enum NewsStatus
{
    Draft,
    Published,
    Archived
}
