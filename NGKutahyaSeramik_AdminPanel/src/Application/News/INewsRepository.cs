namespace Application.News;

public interface INewsRepository
{
    Task<Domain.Entities.News?> GetByIdAsync(int id);

    Task<IReadOnlyList<Domain.Entities.News>> GetAllAsync();

    Task AddAsync(Domain.Entities.News news);

    void Remove(Domain.Entities.News news);

    Task<IReadOnlyList<int>> GetRelatedNewsIdsAsync(int newsId);

    /// <summary>Verilen ilgili haber id listesini newsId'nin ilişkileriyle birebir eşleşecek şekilde değiştirir (ekleme+silme).</summary>
    Task ReplaceRelatedNewsAsync(int newsId, IReadOnlyList<int> relatedNewsIds);

    /// <summary>Haber silinmeden önce, RelatedNewsId Restrict olduğu için bu habere başka haberlerden yapılan referansları temizler.</summary>
    Task RemoveRelatedPostReferencesAsync(int newsId);
}
