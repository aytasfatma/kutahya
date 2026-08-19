using Domain.Entities;

namespace Application.Blogs;

public interface IBlogRepository
{
    Task<Blog?> GetByIdAsync(int id);

    Task<IReadOnlyList<Blog>> GetAllAsync();

    Task AddAsync(Blog blog);

    void Remove(Blog blog);

    Task<IReadOnlyList<Tag>> GetTagsAsync(int blogId);

    /// <summary>Verilen etiket id listesini blog yazısının ilişkileriyle birebir eşleşecek şekilde değiştirir (ekleme+silme).</summary>
    Task ReplaceTagsAsync(int blogId, IReadOnlyList<int> tagIds);

    Task<IReadOnlyList<int>> GetRelatedBlogIdsAsync(int blogId);

    /// <summary>Verilen ilgili yazı id listesini blogId'nin ilişkileriyle birebir eşleşecek şekilde değiştirir (ekleme+silme).</summary>
    Task ReplaceRelatedPostsAsync(int blogId, IReadOnlyList<int> relatedBlogIds);

    /// <summary>Blog silinmeden önce, RelatedBlogId Restrict olduğu için bu blog'a başka yazılardan yapılan referansları temizler.</summary>
    Task RemoveRelatedPostReferencesAsync(int blogId);
}
