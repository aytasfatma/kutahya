namespace Application.ProductImport;

public record ExcelReadResult(IReadOnlyList<string> Headers, IReadOnlyList<ProductImportRow> Rows);

/// <summary>Storage abstraction deseninin (ADR-006/013) Excel okuma/yazma için tekrar kullanımı —
/// Application, belirli bir Excel kütüphanesine (ClosedXML) hiç bağımlı değil, yalnızca bu arayüze.</summary>
public interface IProductImportFileReader
{
    ExcelReadResult Read(Stream fileStream);
}

public interface IProductImportTemplateWriter
{
    byte[] GenerateTemplate();
}
