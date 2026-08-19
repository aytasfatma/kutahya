using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Madde 24 — Katalog/Doküman. Her kayıt tek bir dile ait tek bir fiziksel PDF dosyasını temsil eder
/// (Madde 18.3'ün "urunKodu_dokumanTipi_dil.pdf" isimlendirme standardı — aynı dokümanın TR/EN sürümleri
/// ayrı Document satırlarıdır, Translation üzerinden çoklu dil YOK). Product/Collection ile ilişkisi
/// many-to-many'dir (Madde 36.1/36.2) ve opsiyoneldir — "genel seviye" doküman hiçbir ilişkiye sahip olmayabilir.
/// </summary>
public class Document
{
    public int Id { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public ProductBrand Brand { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }

    public int LanguageId { get; private set; }
    public Language Language { get; private set; } = null!;

    public string FilePath { get; private set; } = null!;
    public string OriginalFileName { get; private set; } = null!;
    public string FileExtension { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long FileSize { get; private set; }

    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    private Document()
    {
    }

    public Document(
        DocumentType documentType,
        ProductBrand brand,
        string title,
        string? description,
        int languageId,
        string filePath,
        string originalFileName,
        string fileExtension,
        string contentType,
        long fileSize,
        int displayOrder)
    {
        DocumentType = documentType;
        Brand = brand;
        Title = title;
        Description = description;
        LanguageId = languageId;
        FilePath = filePath;
        OriginalFileName = originalFileName;
        FileExtension = fileExtension;
        ContentType = contentType;
        FileSize = fileSize;
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public void UpdateMetadata(DocumentType documentType, ProductBrand brand, string title, string? description, int languageId, int displayOrder)
    {
        DocumentType = documentType;
        Brand = brand;
        Title = title;
        Description = description;
        LanguageId = languageId;
        DisplayOrder = displayOrder;
    }

    public void UpdateFile(string filePath, string originalFileName, string fileExtension, string contentType, long fileSize)
    {
        FilePath = filePath;
        OriginalFileName = originalFileName;
        FileExtension = fileExtension;
        ContentType = contentType;
        FileSize = fileSize;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
