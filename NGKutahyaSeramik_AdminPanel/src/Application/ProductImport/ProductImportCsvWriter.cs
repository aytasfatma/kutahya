using System.Text;

namespace Application.ProductImport;

public static class ProductImportCsvWriter
{
    private static readonly char[] FormulaTriggerChars = ['=', '+', '-', '@'];
    private static readonly string[] Header =
        ["Excel Satırı", "Ürün Kodu", "Ürün Adı", "Seri", "Kategori", "Sonuç", "Hata", "Uyarı", "Eksik Zorunlu Alan", "Pasif Ürün"];

    public static string ToCsv(ProductImportReportDto report)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(',', Header.Select(Escape)));

        foreach (var row in report.Rows)
        {
            var fields = new[]
            {
                row.RowNumber.ToString(),
                row.ProductCode,
                row.ProductName ?? string.Empty,
                row.SeriesName ?? string.Empty,
                row.CategoryName ?? string.Empty,
                OutcomeLabel(row.Outcome),
                row.ErrorMessage ?? string.Empty,
                row.WarningMessage ?? string.Empty,
                row.HasMissingRequiredField ? "Evet" : "Hayır",
                row.IsInactiveProduct ? "Evet" : "Hayır"
            };

            builder.AppendLine(string.Join(',', fields.Select(Escape)));
        }

        return builder.ToString();
    }

    private static string OutcomeLabel(ProductImportReportOutcome outcome) => outcome switch
    {
        ProductImportReportOutcome.Created => "Eklendi",
        ProductImportReportOutcome.Updated => "Güncellendi",
        ProductImportReportOutcome.Failed => "Başarısız",
        _ => "Atlandı"
    };

    private static string Escape(string value)
    {
        var sanitized = SanitizeAgainstCsvInjection(value ?? string.Empty);
        return sanitized.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? $"\"{sanitized.Replace("\"", "\"\"")}\""
            : sanitized;
    }

    private static string SanitizeAgainstCsvInjection(string value) =>
        value.Length > 0 && FormulaTriggerChars.Contains(value[0]) ? "'" + value : value;
}
