using Domain.Enums;

namespace Application.ProductImport;

public enum ProductImportRowAction
{
    New,
    Update,
    Skipped
}

/// <summary>Bir Excel satırının ham (henüz doğrulanmamış) hâli — hücre değerleri string olarak
/// tutulur, tip dönüşümü/doğrulama `ProductImportService`'te yapılır.</summary>
public class ProductImportRow
{
    public int RowNumber { get; init; }
    public IReadOnlyDictionary<string, string?> RawValues { get; init; } = new Dictionary<string, string?>();
    public string? ProductCode { get; init; }
    public string? ProductName { get; init; }
    public string? Series { get; init; }
    public string? Brand { get; init; }
    public string? CommercialName { get; init; }
    public string? Status { get; init; }
    public string? ProductGroup { get; init; }
    public string? Size { get; init; }
    public string? Unit { get; init; }
    public string? Surface { get; init; }
    public string? Relief { get; init; }
    public string? SpecialSurface { get; init; }
    public string? HasFace { get; init; }
    public string? HasVenue { get; init; }
    public string? Color { get; init; }
    public string? Category { get; init; }
    public string? Thickness { get; init; }
    public string? BodyType { get; init; }
    public string? VValue { get; init; }
    public string? PEI { get; init; }
    public string? RValue { get; init; }
    public string? DeepAbrasion { get; init; }
    public string? ColorMaterial { get; init; }
    public string? Finish { get; init; }
    public string? HeatResistance { get; init; }
    public string? AntiSlip { get; init; }
    public string? UsageArea { get; init; }
    public string? ApplicationArea { get; init; }
    public string? GlazedGranite { get; init; }
    public string? FaceCount { get; init; }
    public string? BoxM2 { get; init; }
    public string? PalletM2 { get; init; }
}

public class ProductImportRowResult
{
    public int RowNumber { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string? ProductName { get; init; }
    public bool IsValid { get; init; }
    public ProductImportRowAction Action { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];

    // Yalnızca IsValid=true satırlar için doldurulur — ImportAsync'in yazma aşaması bunları
    // doğrudan kullanır, satırı ikinci kez parse/çözümlemez (PreviewAsync ile ImportAsync aynı
    // doğrulama sonucunu üretir, deterministik).
    public int? ExistingProductId { get; init; }
    public int? ResolvedCategoryId { get; init; }
    public string? ResolvedCategoryName { get; init; }
    public bool IsUncategorizedCategory { get; init; }
    public int ResolvedCollectionId { get; init; }
    public string? ResolvedCollectionName { get; init; }
    public ProductBrand ResolvedBrand { get; init; }
    public ProductStatus ResolvedStatus { get; init; }
    public string? ResolvedCommercialName { get; init; }
    public string? ResolvedProductGroup { get; init; }
    public string ResolvedSize { get; init; } = string.Empty;
    public string ResolvedUnit { get; init; } = string.Empty;
    public string? ResolvedSurface { get; init; }
    public string? ResolvedRelief { get; init; }
    public string? ResolvedSpecialSurface { get; init; }
    public string? ResolvedColor { get; init; }
    public string? ResolvedColorMaterial { get; init; }
    public string? ResolvedUsageArea { get; init; }
    public string? ResolvedApplicationArea { get; init; }
    public int? ResolvedFaceCount { get; init; }
    public decimal? ResolvedThickness { get; init; }
    public string? ResolvedBodyType { get; init; }
    public string? ResolvedVValue { get; init; }
    public decimal? ResolvedPEI { get; init; }
    public string? ResolvedRValue { get; init; }
    public string? ResolvedDeepAbrasion { get; init; }
    public string? ResolvedFinish { get; init; }
    public bool? ResolvedHeatResistance { get; init; }
    public bool? ResolvedAntiSlip { get; init; }
    public bool? ResolvedGlazedGranite { get; init; }
    public bool? ResolvedHasFace { get; init; }
    public bool? ResolvedHasVenue { get; init; }
    public decimal? ResolvedBoxM2 { get; init; }
    public decimal? ResolvedPalletM2 { get; init; }
}

public class ProductImportParseResultDto
{
    public bool HeaderValid { get; init; }
    public IReadOnlyList<string> HeaderErrors { get; init; } = [];
    public IReadOnlyList<ProductImportRowResult> Rows { get; init; } = [];

    public int TotalRows => Rows.Count;
    public int ImportableRows => Rows.Count(r => r.IsValid);
    public int BlockingErrors => Rows.Count(r => !r.IsValid);
    public int WarningRows => Rows.Count(r => r.Warnings.Count > 0);
    public int NewProducts => Rows.Count(r => r.IsValid && r.Action == ProductImportRowAction.New);
    public int UpdatedProducts => Rows.Count(r => r.IsValid && r.Action == ProductImportRowAction.Update);
    public int TotalWarnings => Rows.Sum(r => r.Warnings.Count);
    public int UncategorizedProducts => Rows.Count(r => r.IsValid && r.IsUncategorizedCategory);
    public int NewCollections => Rows
        .Where(r => r.IsValid && r.ResolvedCollectionId == 0 && !string.IsNullOrWhiteSpace(r.ResolvedCollectionName))
        .Select(r => r.ResolvedCollectionName)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Count();
}

/// <summary>10. adım — Sonuç raporu: Toplam/Başarılı/Hatalı/Yeni eklenen/Güncellenen/Atlanan.</summary>
public class ProductImportResultDto
{
    public int Total { get; init; }
    public int Succeeded { get; init; }
    public int Failed { get; init; }
    public int Created { get; init; }
    public int Updated { get; init; }
    public int Skipped { get; init; }
    public IReadOnlyList<ProductImportRowResult> RowResults { get; init; } = [];

    /// <summary>Madde 37.3 raporlama katmanı için eklendi (mevcut import pipeline'ı değişmedi —
    /// yalnızca zaten var olan try/catch'in transaction sonucunu dışarı açan bir bayrak). true ise
    /// `RowResults`'taki bireysel `IsValid=true` satırlar bile GERÇEKTE yazılmamıştır (transaction
    /// geri alındı) — rapor katmanı bunu Outcome=Failed olarak yansıtmalıdır, aksi hâlde "hiçbir şey
    /// kaydedilmedi" durumunda satırlar yanlışlıkla "Yeni/Güncellendi" görünür.</summary>
    public bool TransactionRolledBack { get; init; }
}
