using Domain.Enums;

namespace Application.Documents;

public static class DocumentEnumDisplay
{
    public static string GetDocumentTypeLabel(DocumentType type) => type switch
    {
        DocumentType.Catalog => "Katalog",
        DocumentType.TechnicalSheet => "Teknik Föy",
        DocumentType.Certificate => "Sertifika",
        DocumentType.Report => "Rapor",
        _ => type.ToString()
    };

    /// <summary>Klasör segmenti — Madde 35.4/ADR-006 terminolojisiyle tutarlı (catalog/documents/certificate/reports).</summary>
    public static string GetFolderSegment(DocumentType type) => type switch
    {
        DocumentType.Catalog => "catalog",
        DocumentType.TechnicalSheet => "documents",
        DocumentType.Certificate => "certificate",
        DocumentType.Report => "reports",
        _ => "other"
    };

    public static string FormatFileSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double size = bytes;
        var unitIndex = 0;
        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {units[unitIndex]}";
    }
}
