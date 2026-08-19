namespace Application.ProductImport;

public enum ProductCatalogResetIssueLevel
{
    BlockingError,
    RowWarning,
    Information
}

public class ProductCatalogResetIssueDto
{
    public ProductCatalogResetIssueLevel Level { get; init; }
    public int? RowNumber { get; init; }
    public string? ProductCode { get; init; }
    public string Message { get; init; } = string.Empty;
}

public class ProductCatalogResetDryRunDto
{
    public string BatchId { get; init; } = string.Empty;
    public string SourceFilePath { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string FileHash { get; init; } = string.Empty;
    public DateTime StartedAtUtc { get; init; }
    public DateTime FinishedAtUtc { get; init; }
    public string RequestedBy { get; init; } = string.Empty;
    public int TotalRows { get; init; }
    public int ImportableProducts { get; init; }
    public int BlockingErrorRows { get; init; }
    public int WarningRows { get; init; }
    public int TotalWarnings { get; init; }
    public int UncategorizedProducts { get; init; }
    public int RealCategoryCount { get; init; }
    public int SpecialCategoryCount { get; init; }
    public int TotalCategoryCount => RealCategoryCount + SpecialCategoryCount;
    public int CollectionCount { get; init; }
    public int ProductCount { get; init; }
    public int DuplicateProductCodeCount { get; init; }
    public int UnknownHeaderCount { get; init; }
    public IReadOnlyList<string> RealCategories { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CollectionsSample { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> BrandValues { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ProductCatalogResetIssueDto> Issues { get; init; } = Array.Empty<ProductCatalogResetIssueDto>();
    public IReadOnlyList<string> NormalizationExamples { get; init; } = Array.Empty<string>();
}

public record ProductCatalogResetResultDto
{
    public string BatchId { get; init; } = string.Empty;
    public string FileHash { get; init; } = string.Empty;
    public DateTime StartedAtUtc { get; init; }
    public DateTime FinishedAtUtc { get; init; }
    public string RequestedBy { get; init; } = string.Empty;
    public bool Succeeded { get; init; }
    public bool TransactionRolledBack { get; init; }
    public string? BackupPath { get; init; }
    public string? ResultPath { get; init; }
    public int DeletedProducts { get; init; }
    public int DeletedCategories { get; init; }
    public int DeletedCollections { get; init; }
    public int AddedProducts { get; init; }
    public int AddedCategories { get; init; }
    public int AddedCollections { get; init; }
    public int AddedProductTranslations { get; init; }
    public int AddedCategoryTranslations { get; init; }
    public int AddedCollectionTranslations { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}

public interface IProductCatalogResetService
{
    Task<ProductCatalogResetDryRunDto> DryRunAsync(Stream fileStream, string originalFileName, string requestedBy);
    Task<ProductCatalogResetDryRunDto> LoadDryRunAsync(string sourceFilePath);
    Task<ProductCatalogResetResultDto> ResetAsync(string sourceFilePath, string expectedFileHash, string confirmationText, string requestedBy);
}
