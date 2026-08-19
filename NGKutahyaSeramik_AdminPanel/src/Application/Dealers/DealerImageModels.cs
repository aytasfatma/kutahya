using Domain.Entities;

namespace Application.Dealers;

public sealed record DealerImageDto(int Id, int DealerId, string FilePath, bool IsFeatured, int DisplayOrder);

public sealed class AddDealerImageRequest
{
    public int DealerId { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Length { get; init; }
    public Stream Content { get; init; } = Stream.Null;
}

public interface IDealerImageRepository
{
    Task<DealerImage?> GetByIdAsync(int id);
    Task<IReadOnlyList<DealerImage>> GetByDealerIdAsync(int dealerId);
    Task AddAsync(DealerImage image);
    void Remove(DealerImage image);
    void RemoveRange(IEnumerable<DealerImage> images);
}
