namespace Domain.Entities;

/// <summary>
/// Blog yazısının kendi kendine ilişki tablosu ("Benzer Hikâyeler" bölümü — many-to-many, self-referencing).
/// Yön tektir: BlogId yazısının detay sayfasında RelatedBlogId yazısı önerilir. Karşılıklı önerilmek
/// istenirse iki satır (A→B, B→A) ayrı ayrı eklenir. Blog silinirse bu ilişki satırı da (her iki
/// yönde) Cascade FK ile otomatik silinir.
/// </summary>
public class BlogRelatedPost
{
    public int Id { get; private set; }
    public int BlogId { get; private set; }
    public Blog Blog { get; private set; } = null!;
    public int RelatedBlogId { get; private set; }
    public Blog RelatedBlog { get; private set; } = null!;

    private BlogRelatedPost()
    {
    }

    public BlogRelatedPost(int blogId, int relatedBlogId)
    {
        BlogId = blogId;
        RelatedBlogId = relatedBlogId;
    }
}
