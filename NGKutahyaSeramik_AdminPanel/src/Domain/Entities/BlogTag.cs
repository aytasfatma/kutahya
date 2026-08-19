namespace Domain.Entities;

/// <summary>
/// Blog-etiket ilişki tablosu (Madde 21.1 "Tags: array" → many-to-many, ProductDocument/
/// ProductReferenceProject ile aynı junction deseni). Cascade FK her iki yönde — Blog veya Tag
/// silinirse yalnızca ilişki satırı gider.
/// </summary>
public class BlogTag
{
    public int Id { get; private set; }
    public int BlogId { get; private set; }
    public Blog Blog { get; private set; } = null!;
    public int TagId { get; private set; }
    public Tag Tag { get; private set; } = null!;

    private BlogTag()
    {
    }

    public BlogTag(int blogId, int tagId)
    {
        BlogId = blogId;
        TagId = tagId;
    }
}
