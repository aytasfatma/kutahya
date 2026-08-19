using Domain.Enums;

namespace Application.Documents;

public class CreateDocumentRequest
{
    public DocumentType DocumentType { get; init; }
    public ProductBrand Brand { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int LanguageId { get; init; }
    public int DisplayOrder { get; init; }
    public IReadOnlyList<int> ProductIds { get; init; } = [];
    public IReadOnlyList<int> CollectionIds { get; init; } = [];

    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Length { get; init; }
    public Stream Content { get; init; } = Stream.Null;
}

public class UpdateDocumentRequest
{
    public DocumentType DocumentType { get; init; }
    public ProductBrand Brand { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int LanguageId { get; init; }
    public int DisplayOrder { get; init; }
    public IReadOnlyList<int> ProductIds { get; init; } = [];
    public IReadOnlyList<int> CollectionIds { get; init; } = [];

    /// <summary>Dosya değişimi opsiyoneldir — null bırakılırsa mevcut fiziksel dosya korunur.</summary>
    public string? OriginalFileName { get; init; }
    public string? ContentType { get; init; }
    public long? Length { get; init; }
    public Stream? Content { get; init; }
}
