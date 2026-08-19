using Domain.Enums;

namespace Application.ProductImport;

/// <summary>
/// Backlog #17 devamı — Madde 37.3 (Import Sonrası Raporlama). Mevcut import pipeline'ına (parse/
/// validate/write — `ProductImportService.ImportAsync`) hiç dokunmuyor; yalnızca onun ürettiği
/// `ProductImportResultDto`'yu GÖZLEMLEYİP raporlama-amaçlı bir sınıflandırma üretiyor. Saf, DB/dosya
/// bağımlılığı olmayan statik bir dönüştürücü — bağımsız test edilebilir.
/// </summary>
public static class ProductImportReportBuilder
{
    private const string RequiredFieldErrorSuffix = "zorunludur.";

    public static ProductImportReportDto Build(string fileName, ProductImportResultDto result)
    {
        var rows = result.RowResults.Select(row => BuildRow(row, result.TransactionRolledBack)).ToList();

        return new ProductImportReportDto
        {
            GeneratedAtUtc = DateTime.UtcNow,
            FileName = fileName,
            Rows = rows
        };
    }

    private static ProductImportReportRow BuildRow(ProductImportRowResult row, bool transactionRolledBack)
    {
        var outcome = DetermineOutcome(row, transactionRolledBack);
        var errorMessage = row.Errors.Count > 0 ? string.Join(" ", row.Errors) : null;
        var warningMessage = row.Warnings.Count > 0 ? string.Join(" ", row.Warnings) : null;

        if (transactionRolledBack && row.IsValid)
        {
            errorMessage = "İçe aktarma sırasında beklenmeyen bir hata oluştu; tüm işlem geri alındı, bu satır kaydedilmedi.";
        }

        var hasMissingRequiredField = row.Errors.Any(e => e.EndsWith(RequiredFieldErrorSuffix, StringComparison.Ordinal));
        var isInactiveProduct = outcome is ProductImportReportOutcome.Created or ProductImportReportOutcome.Updated
            && row.ResolvedStatus == ProductStatus.Inactive;

        return new ProductImportReportRow
        {
            RowNumber = row.RowNumber,
            ProductCode = row.ProductCode,
            ProductName = row.ProductName,
            SeriesName = row.ResolvedCollectionName,
            CategoryName = row.ResolvedCategoryName,
            Outcome = outcome,
            ErrorMessage = errorMessage,
            WarningMessage = warningMessage,
            HasMissingRequiredField = hasMissingRequiredField,
            IsInactiveProduct = isInactiveProduct
        };
    }

    private static ProductImportReportOutcome DetermineOutcome(ProductImportRowResult row, bool transactionRolledBack)
    {
        if (transactionRolledBack)
        {
            return ProductImportReportOutcome.Failed;
        }

        if (!row.IsValid)
        {
            return ProductImportReportOutcome.Failed;
        }

        return row.Action switch
        {
            ProductImportRowAction.New => ProductImportReportOutcome.Created,
            ProductImportRowAction.Update => ProductImportReportOutcome.Updated,
            _ => ProductImportReportOutcome.Skipped
        };
    }
}
