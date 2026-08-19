namespace Application.ProductImport;

/// <summary>Madde 37.3 — "Sonuç tipi" için raporlama katmanının kendi sınıflandırması. Mevcut
/// `ProductImportRowAction` (parse-zamanı, New/Update/Skipped) ile birebir aynı değil: bu, satırın
/// GERÇEKTE ne olduğunu (transaction rollback dahil) yansıtır.</summary>
public enum ProductImportReportOutcome
{
    Created,
    Updated,
    Failed,
    Skipped
}

/// <summary>Raporda en az bulunması istenen alanlar: Excel satır numarası, ProductCode, Sonuç tipi,
/// Hata açıklaması — artı Madde 37.3'ün "eksik zorunlu alan"/"pasif ürün" raporları için gereken
/// bayraklar.</summary>
public class ProductImportReportRow
{
    public int RowNumber { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string? ProductName { get; init; }
    public string? SeriesName { get; init; }
    public string? CategoryName { get; init; }
    public ProductImportReportOutcome Outcome { get; init; }
    public string? ErrorMessage { get; init; }
    public string? WarningMessage { get; init; }
    public bool HasMissingRequiredField { get; init; }
    public bool IsInactiveProduct { get; init; }
}

public class ProductImportReportDto
{
    public DateTime GeneratedAtUtc { get; init; }
    public string FileName { get; init; } = string.Empty;
    public IReadOnlyList<ProductImportReportRow> Rows { get; init; } = [];

    public int Total => Rows.Count;
    public int Created => Rows.Count(r => r.Outcome == ProductImportReportOutcome.Created);
    public int Updated => Rows.Count(r => r.Outcome == ProductImportReportOutcome.Updated);
    public int Failed => Rows.Count(r => r.Outcome == ProductImportReportOutcome.Failed);
    public int Skipped => Rows.Count(r => r.Outcome == ProductImportReportOutcome.Skipped);
    public int Succeeded => Created + Updated;
    public int WarningRows => Rows.Count(r => !string.IsNullOrWhiteSpace(r.WarningMessage));

    /// <summary>Hatalı kayıt raporu.</summary>
    public IReadOnlyList<ProductImportReportRow> FailedRows =>
        Rows.Where(r => r.Outcome == ProductImportReportOutcome.Failed).ToList();

    /// <summary>Eksik zorunlu alan raporu.</summary>
    public IReadOnlyList<ProductImportReportRow> MissingRequiredFieldRows =>
        Rows.Where(r => r.HasMissingRequiredField).ToList();

    /// <summary>Pasif ürün raporu — bu import'ta oluşturulan/güncellenen ve Status=Pasif olan ürünler.</summary>
    public IReadOnlyList<ProductImportReportRow> InactiveProductRows =>
        Rows.Where(r => r.IsInactiveProduct).ToList();

    /// <summary>Yeni eklenen kayıt özeti.</summary>
    public IReadOnlyList<ProductImportReportRow> CreatedRows =>
        Rows.Where(r => r.Outcome == ProductImportReportOutcome.Created).ToList();

    /// <summary>Güncellenen kayıt özeti.</summary>
    public IReadOnlyList<ProductImportReportRow> UpdatedRows =>
        Rows.Where(r => r.Outcome == ProductImportReportOutcome.Updated).ToList();
}
