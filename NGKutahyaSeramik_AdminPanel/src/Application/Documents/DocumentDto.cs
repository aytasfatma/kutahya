using Domain.Enums;

namespace Application.Documents;

public class DocumentRelatedItemDto
{
    public int Id { get; init; }
    public string Label { get; init; } = string.Empty;
}

public class DocumentDto
{
    public int Id { get; init; }
    public DocumentType DocumentType { get; init; }
    public ProductBrand Brand { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }

    public int LanguageId { get; init; }
    public string LanguageCode { get; init; } = string.Empty;
    public string LanguageName { get; init; } = string.Empty;

    public string FilePath { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string FileExtension { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }

    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }

    public IReadOnlyList<DocumentRelatedItemDto> RelatedProducts { get; init; } = [];
    public IReadOnlyList<DocumentRelatedItemDto> RelatedCollections { get; init; } = [];

    public string DocumentTypeLabel => DocumentEnumDisplay.GetDocumentTypeLabel(DocumentType);
    public string FileSizeLabel => DocumentEnumDisplay.FormatFileSize(FileSize);
}
