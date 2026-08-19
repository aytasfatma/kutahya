namespace Domain.Entities;

/// <summary>
/// Haberin kendi kendine ilişki tablosu ("İlgili Haberler" bölümü — many-to-many, self-referencing).
/// BlogRelatedPost ile birebir aynı desen: NewsId'nin detay sayfasında RelatedNewsId önerilir, yön tektir.
/// </summary>
public class NewsRelatedPost
{
    public int Id { get; private set; }
    public int NewsId { get; private set; }
    public News News { get; private set; } = null!;
    public int RelatedNewsId { get; private set; }
    public News RelatedNews { get; private set; } = null!;

    private NewsRelatedPost()
    {
    }

    public NewsRelatedPost(int newsId, int relatedNewsId)
    {
        NewsId = newsId;
        RelatedNewsId = relatedNewsId;
    }
}
