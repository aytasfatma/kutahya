using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Madde 21/21.1 — Blog Modülü. Çevrilebilir alanlar (Title/Excerpt/Content/SeoUrl(Slug)/MetaTitle/
/// MetaDescription) Translation'da (EntityType.Blog); Category/Author/PublishDate/Status/FeaturedImage
/// native. Madde 21.1'in Zorunluluk sütunu olmadığı için (ReferenceProject ile aynı durum) yalnızca
/// TR Title zorunlu tutuldu; DisplayOrder eklenmedi — blog akışı doküman gereği (Madde 15.1 "son
/// eklenen") PublishDate'e göre kronolojik sıralanıyor, manuel sıralama istenmiyor.
/// </summary>
public class Blog
{
    public int Id { get; private set; }

    public int? BlogCategoryId { get; private set; }
    public BlogCategory? BlogCategory { get; private set; }

    public string? Author { get; private set; }
    public DateTime? PublishDate { get; private set; }
    public BlogStatus Status { get; private set; }
    public bool IsTrend { get; private set; }
    public string? FeaturedImagePath { get; private set; }
    public string? SecondaryImagePath { get; private set; }

    private Blog()
    {
    }

    public Blog(int? blogCategoryId, string? author, DateTime? publishDate, BlogStatus status, bool isTrend = false)
    {
        BlogCategoryId = blogCategoryId;
        Author = author;
        PublishDate = publishDate;
        Status = status;
        IsTrend = isTrend;
    }

    public void UpdateDetails(int? blogCategoryId, string? author, DateTime? publishDate, BlogStatus status, bool isTrend = false)
    {
        BlogCategoryId = blogCategoryId;
        Author = author;
        PublishDate = publishDate;
        Status = status;
        IsTrend = isTrend;
    }

    public void SetFeaturedImagePath(string? filePath) => FeaturedImagePath = filePath;
    public void SetSecondaryImagePath(string? filePath) => SecondaryImagePath = filePath;
}
