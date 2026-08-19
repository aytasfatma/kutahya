using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Madde 22 — Haberler Modülü. "Haber veri modeli blog modülü ile benzer yapıda olacaktır: başlık,
/// içerik, kapak görseli, kategori, yayın tarihi, durum ve SEO alanları." Blog'un aksine Excerpt/
/// Author/Tags **yok** — doküman bunları Haber için hiç anmıyor, Blog'dan körü körüne kopyalanmadı.
/// Çevrilebilir alanlar (Title/Content/SeoUrl/MetaTitle/MetaDescription) Translation'da (EntityType.News);
/// NewsCategory/PublishDate/Status/FeaturedImage native. DisplayOrder yok — Blog ile aynı gerekçe
/// (haber akışı PublishDate'e göre kronolojik).
/// </summary>
public class News
{
    public int Id { get; private set; }

    public int? NewsCategoryId { get; private set; }
    public NewsCategory? NewsCategory { get; private set; }

    public DateTime? PublishDate { get; private set; }
    public NewsStatus Status { get; private set; }
    public string? FeaturedImagePath { get; private set; }

    private News()
    {
    }

    public News(int? newsCategoryId, DateTime? publishDate, NewsStatus status)
    {
        NewsCategoryId = newsCategoryId;
        PublishDate = publishDate;
        Status = status;
    }

    public void UpdateDetails(int? newsCategoryId, DateTime? publishDate, NewsStatus status)
    {
        NewsCategoryId = newsCategoryId;
        PublishDate = publishDate;
        Status = status;
    }

    public void SetFeaturedImagePath(string? filePath) => FeaturedImagePath = filePath;
}
