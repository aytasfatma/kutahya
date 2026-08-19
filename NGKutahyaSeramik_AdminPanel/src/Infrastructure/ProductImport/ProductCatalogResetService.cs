using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Categories;
using Application.Collections;
using Application.ProductImport;
using Application.Products;
using Application.Storage;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.ProductImport;

public class ProductCatalogResetService : IProductCatalogResetService
{
    private const string TrLanguageCode = "TR";
    private const string UncategorizedCategoryKey = "__uncategorized__";
    private const string ConfirmationPhrase = "KATALOĞU YENİDEN OLUŞTUR";

    private readonly AppDbContext _dbContext;
    private readonly IProductImportFileReader _fileReader;
    private readonly IFileStorageService _fileStorageService;
    private readonly ProductImportNormalizer _normalizer = new();

    public ProductCatalogResetService(
        AppDbContext dbContext,
        IProductImportFileReader fileReader,
        IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _fileReader = fileReader;
        _fileStorageService = fileStorageService;
    }

    public async Task<ProductCatalogResetDryRunDto> DryRunAsync(Stream fileStream, string originalFileName, string requestedBy)
    {
        var batchId = Guid.NewGuid().ToString("N");
        await using var memory = new MemoryStream();
        fileStream.Position = 0;
        await fileStream.CopyToAsync(memory);
        var bytes = memory.ToArray();
        var hash = ComputeHash(bytes);

        await using var sourceStream = new MemoryStream(bytes);
        var sourcePath = await _fileStorageService.SaveAsync($"imports/products/{batchId}", sourceStream, "source.xlsx");

        await using var readStream = new MemoryStream(bytes);
        var dryRun = BuildDryRun(readStream, batchId, sourcePath, originalFileName, hash, requestedBy);
        await SaveJsonAsync($"imports/products/{batchId}", "dry-run.json", dryRun);
        return dryRun;
    }

    public async Task<ProductCatalogResetDryRunDto> LoadDryRunAsync(string sourceFilePath)
    {
        await using var stream = await _fileStorageService.OpenReadAsync(sourceFilePath);
        var batchId = ResolveBatchId(sourceFilePath);
        var hash = await ComputeHashAsync(stream);
        stream.Position = 0;
        return BuildDryRun(stream, batchId, sourceFilePath, Path.GetFileName(sourceFilePath), hash, string.Empty);
    }

    public async Task<ProductCatalogResetResultDto> ResetAsync(string sourceFilePath, string expectedFileHash, string confirmationText, string requestedBy)
    {
        var startedAt = DateTime.UtcNow;
        var batchId = ResolveBatchId(sourceFilePath);

        if (!string.Equals(confirmationText?.Trim(), ConfirmationPhrase, StringComparison.Ordinal))
        {
            return Failed(batchId, expectedFileHash, startedAt, requestedBy, "Onay metni eşleşmedi.");
        }

        await using var stream = await _fileStorageService.OpenReadAsync(sourceFilePath);
        var hash = await ComputeHashAsync(stream);
        if (!string.Equals(hash, expectedFileHash, StringComparison.OrdinalIgnoreCase))
        {
            return Failed(batchId, hash, startedAt, requestedBy, "Excel dosyası hash doğrulaması başarısız.");
        }

        stream.Position = 0;
        var dryRun = BuildDryRun(stream, batchId, sourceFilePath, Path.GetFileName(sourceFilePath), hash, requestedBy);
        if (dryRun.BlockingErrorRows > 0 || dryRun.DuplicateProductCodeCount > 0)
        {
            return Failed(batchId, hash, startedAt, requestedBy, "Bloklayıcı hata bulunan dry-run import edilemez.");
        }

        stream.Position = 0;
        var rows = BuildImportRows(_fileReader.Read(stream).Rows).ToList();
        var trLanguage = await _dbContext.Languages.FirstAsync(l => l.Code == TrLanguageCode);

        var backup = await BuildBackupAsync();
        var backupPath = await SaveJsonAsync($"backups/product-catalog/{batchId}", "before-reset.json", backup);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var deletedProducts = await _dbContext.Products.CountAsync();
            var deletedCategories = await _dbContext.Categories.CountAsync();
            var deletedCollections = await _dbContext.Collections.CountAsync();

            await _dbContext.Translations
                .Where(t => t.EntityType == EntityType.Product || t.EntityType == EntityType.Category || t.EntityType == EntityType.Collection)
                .ExecuteDeleteAsync();
            await _dbContext.ProductDocuments.ExecuteDeleteAsync();
            await _dbContext.ProductReferenceProjects.ExecuteDeleteAsync();
            await _dbContext.ProductImages.ExecuteDeleteAsync();
            await _dbContext.CollectionDocuments.ExecuteDeleteAsync();
            await _dbContext.Products.ExecuteDeleteAsync();
            await _dbContext.Surfaces.ExecuteDeleteAsync();
            await _dbContext.Categories.ExecuteDeleteAsync();
            await _dbContext.Collections.ExecuteDeleteAsync();

            var categoryMap = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
            var categoryNames = rows
                .Where(r => r.CategoryKey != UncategorizedCategoryKey)
                .Select(r => r.CategoryName!)
                .DistinctBy(ReferenceKey)
                .OrderBy(r => r, StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), ignoreCase: true))
                .ToList();

            var displayOrder = 1;
            foreach (var name in categoryNames)
            {
                var category = new Category(null, null, displayOrder++);
                category.SetIdentity(name, null);
                _dbContext.Categories.Add(category);
                categoryMap[ReferenceKey(name)] = category;
            }

            var collectionMap = new Dictionary<string, Collection>(StringComparer.OrdinalIgnoreCase);
            var collectionNames = rows
                .Select(r => r.SeriesName)
                .DistinctBy(ReferenceKey)
                .OrderBy(r => r, StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), ignoreCase: true))
                .ToList();

            displayOrder = 1;
            foreach (var name in collectionNames)
            {
                var collection = new Collection(null, displayOrder++);
                collection.SetIdentity(name, null);
                _dbContext.Collections.Add(collection);
                collectionMap[ReferenceKey(name)] = collection;
            }

            var surfaceMap = new Dictionary<string, Surface>(StringComparer.OrdinalIgnoreCase);
            var surfaceNames = rows
                .Where(r => !string.IsNullOrWhiteSpace(r.Surface))
                .Select(r => r.Surface!.Trim())
                .DistinctBy(ReferenceKey)
                .OrderBy(r => r, StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), true))
                .ToList();
            displayOrder = 1;
            foreach (var name in surfaceNames)
            {
                var surface = new Surface(name, displayOrder++);
                _dbContext.Surfaces.Add(surface);
                surfaceMap[ReferenceKey(name)] = surface;
            }

            await _dbContext.SaveChangesAsync();

            var productDisplayOrder = 1;
            var productTranslations = new List<(Product Product, string Name)>();
            foreach (var row in rows)
            {
                var product = new Product(
                    row.ProductCode,
                    row.CategoryKey == UncategorizedCategoryKey ? null : categoryMap[row.CategoryKey].Id,
                    collectionMap[row.SeriesKey].Id,
                    row.Brand,
                    row.Status,
                    row.CommercialName,
                    row.ProductGroup,
                    row.Size,
                    row.Unit,
                    row.Surface,
                    row.Relief,
                    row.SpecialSurface,
                    row.FaceCount,
                    row.Thickness,
                    row.BodyType,
                    row.Color,
                    row.ColorMaterial,
                    row.ApplicationArea,
                    row.UsageArea,
                    row.Finish,
                    row.PEI,
                    row.VValue,
                    row.RValue,
                    row.DeepAbrasion,
                    row.HeatResistance,
                    row.AntiSlip,
                    row.GlazedGranite,
                    row.HasFace,
                    row.HasVenue,
                    row.BoxM2,
                    row.PalletM2,
                    productDisplayOrder++);

                if (!string.IsNullOrWhiteSpace(row.Surface))
                    product.SetSurface(surfaceMap[ReferenceKey(row.Surface)].Id, row.Surface);

                _dbContext.Products.Add(product);
                productTranslations.Add((product, row.ProductName));
            }

            await _dbContext.SaveChangesAsync();
            foreach (var (product, name) in productTranslations)
            {
                _dbContext.Translations.Add(new Translation(EntityType.Product, product.Id, trLanguage.Id, ProductFields.Name, name));
            }

            await _dbContext.SaveChangesAsync();
            await ValidateFinalStateAsync(rows.Count, categoryNames.Count, collectionNames.Count);
            await transaction.CommitAsync();

            var result = new ProductCatalogResetResultDto
            {
                BatchId = batchId,
                FileHash = hash,
                StartedAtUtc = startedAt,
                FinishedAtUtc = DateTime.UtcNow,
                RequestedBy = requestedBy,
                Succeeded = true,
                BackupPath = backupPath,
                DeletedProducts = deletedProducts,
                DeletedCategories = deletedCategories,
                DeletedCollections = deletedCollections,
                AddedProducts = rows.Count,
                AddedCategories = categoryNames.Count,
                AddedCollections = collectionNames.Count,
                AddedProductTranslations = rows.Count,
                AddedCategoryTranslations = categoryNames.Count,
                AddedCollectionTranslations = collectionNames.Count,
                Warnings = dryRun.Issues.Where(i => i.Level == ProductCatalogResetIssueLevel.RowWarning).Select(i => i.Message).ToList()
            };

            var resultPath = await SaveJsonAsync($"imports/products/{batchId}", "result.json", result);
            return WithResultPath(result, resultPath);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            var result = Failed(batchId, hash, startedAt, requestedBy, ex.Message) with { BackupPath = backupPath, TransactionRolledBack = true };
            await SaveJsonAsync($"imports/products/{batchId}", "result.json", result);
            return result;
        }
    }

    private ProductCatalogResetDryRunDto BuildDryRun(Stream stream, string batchId, string sourcePath, string originalFileName, string hash, string requestedBy)
    {
        var startedAt = DateTime.UtcNow;
        var read = _fileReader.Read(stream);
        var unknownHeaders = read.Headers.Count(h => ProductImportHeaders.ResolveCanonical(h) is null);
        var rows = BuildImportRows(read.Rows).ToList();
        var issues = rows.SelectMany(r => r.Issues).ToList();
        var blockingRows = issues.Where(i => i.Level == ProductCatalogResetIssueLevel.BlockingError && i.RowNumber.HasValue).Select(i => i.RowNumber!.Value).Distinct().Count();
        var warningRows = issues.Where(i => i.Level == ProductCatalogResetIssueLevel.RowWarning && i.RowNumber.HasValue).Select(i => i.RowNumber!.Value).Distinct().Count();
        var duplicateCount = rows.GroupBy(r => ReferenceKey(r.ProductCode)).Count(g => g.Count() > 1);
        var realCategories = rows.Where(r => r.CategoryKey != UncategorizedCategoryKey)
            .Select(r => r.CategoryName!)
            .DistinctBy(ReferenceKey)
            .OrderBy(v => v)
            .ToList();

        return new ProductCatalogResetDryRunDto
        {
            BatchId = batchId,
            SourceFilePath = sourcePath,
            OriginalFileName = originalFileName,
            FileHash = hash,
            StartedAtUtc = startedAt,
            FinishedAtUtc = DateTime.UtcNow,
            RequestedBy = requestedBy,
            TotalRows = read.Rows.Count,
            ImportableProducts = read.Rows.Count - blockingRows,
            BlockingErrorRows = blockingRows,
            WarningRows = warningRows,
            TotalWarnings = issues.Count(i => i.Level == ProductCatalogResetIssueLevel.RowWarning),
            UncategorizedProducts = rows.Count(r => r.CategoryKey == UncategorizedCategoryKey),
            RealCategoryCount = realCategories.Count,
            SpecialCategoryCount = 0,
            CollectionCount = rows.Select(r => r.SeriesKey).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            ProductCount = rows.Count,
            DuplicateProductCodeCount = duplicateCount,
            UnknownHeaderCount = unknownHeaders,
            RealCategories = realCategories,
            CollectionsSample = rows.Select(r => r.SeriesName).DistinctBy(ReferenceKey).Take(50).ToList(),
            BrandValues = rows.Select(r => r.RawBrand).Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!).DistinctBy(ReferenceKey).ToList(),
            Issues = issues,
            NormalizationExamples =
            [
                "60*120 -> 60x120",
                "8,0 mm -> 8.0",
                "boş / - -> null",
                "Kategori boş -> kategori seçilmedi",
                "Face Sayısı yok -> null",
                "HAYIR (SIRSIZ) -> false"
            ]
        };
    }

    private IEnumerable<ResetRow> BuildImportRows(IReadOnlyList<ProductImportRow> rows)
    {
        var duplicateKeys = rows
            .Where(r => !string.IsNullOrWhiteSpace(_normalizer.NullIfBlankOrDash(r.ProductCode)))
            .GroupBy(r => ReferenceKey(r.ProductCode!))
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var issues = new List<ProductCatalogResetIssueDto>();
            var productCode = Required(row.ProductCode, row, "Ürün Kodu", issues);
            var productName = Required(row.ProductName, row, "Ürün Adı", issues);
            var series = Required(row.Series, row, "Seri Adı", issues);
            var brand = ParseBrand(row.Brand, row, issues);

            if (!string.IsNullOrWhiteSpace(productCode) && duplicateKeys.Contains(ReferenceKey(productCode)))
            {
                AddIssue(issues, ProductCatalogResetIssueLevel.BlockingError, row, $"Ürün Kodu dosya içinde tekrarlanıyor: {productCode}");
            }

            var category = _normalizer.NullIfBlankOrDash(row.Category);
            var categoryKey = category is null ? UncategorizedCategoryKey : ReferenceKey(category);
            if (category is null)
            {
                AddIssue(issues, ProductCatalogResetIssueLevel.RowWarning, row, "Excel’de kategori bilgisi bulunmadığı için ürün kategori seçilmeden aktarılacaktır.");
            }

            var status = ParseStatus(row.Status, row, issues);
            var surface = OptionalText(row.Surface, row, issues, "Yüzey bilgisi bulunmadığı için boş bırakıldı.");
            var color = OptionalText(row.Color, row, issues, "Renk bilgisi bulunmadığı için boş bırakıldı.");
            var applicationArea = OptionalArea(row.ApplicationArea, row, issues, "Uygulama alanı bilgisi bulunmadığı için boş bırakıldı.", _normalizer.NormalizeApplicationArea);
            var usageArea = OptionalArea(row.UsageArea, row, issues, "Kullanım alanı bilgisi bulunmadığı için boş bırakıldı.", _normalizer.NormalizeUsageArea);
            var faceCount = ParseFaceCount(row.FaceCount, row, issues);
            var glazedGranite = ParseGlazedGranite(row.GlazedGranite, row, issues);
            var rValue = ParseRValue(row.RValue, row, issues);
            var antiSlip = ParseAntiSlip(row.AntiSlip, rValue, row, issues);

            yield return new ResetRow(
                row.RowNumber,
                productCode,
                productName,
                series,
                ReferenceKey(series),
                row.Brand,
                brand,
                category,
                categoryKey,
                status,
                _normalizer.NullIfBlankOrDash(row.CommercialName),
                _normalizer.NullIfBlankOrDash(row.ProductGroup),
                _normalizer.NormalizeSize(row.Size) ?? string.Empty,
                _normalizer.NormalizeUnit(row.Unit) ?? "M2",
                surface,
                _normalizer.NullIfBlankOrDash(row.Relief),
                _normalizer.NullIfBlankOrDash(row.SpecialSurface),
                faceCount,
                ParseNullableDecimal(row.Thickness, row, issues, "Kalınlık"),
                _normalizer.NullIfBlankOrDash(row.BodyType),
                color,
                _normalizer.NullIfBlankOrDash(row.ColorMaterial),
                applicationArea,
                usageArea,
                _normalizer.NullIfBlankOrDash(row.Finish),
                ParsePEI(row.PEI, row, issues),
                _normalizer.NormalizeVValue(row.VValue),
                rValue ?? antiSlip.RValue,
                _normalizer.NullIfBlankOrDash(row.DeepAbrasion),
                ParseBoolean(row.HeatResistance, row, issues, "Isıya Dayanıklılık"),
                antiSlip.AntiSlip,
                glazedGranite,
                ParseBoolean(row.HasFace, row, issues, "Face"),
                ParseBoolean(row.HasVenue, row, issues, "Mekan"),
                ParseNullableDecimal(row.BoxM2, row, issues, "Kutu m²"),
                ParseNullableDecimal(row.PalletM2, row, issues, "Palet m²"),
                issues);
        }
    }

    private string Required(string? value, ProductImportRow row, string label, List<ProductCatalogResetIssueDto> issues)
    {
        var text = _normalizer.NullIfBlankOrDash(value);
        if (text is null)
        {
            AddIssue(issues, ProductCatalogResetIssueLevel.BlockingError, row, $"{label} boş olamaz.");
        }
        return text ?? string.Empty;
    }

    private ProductBrand ParseBrand(string? value, ProductImportRow row, List<ProductCatalogResetIssueDto> issues)
    {
        var brand = _normalizer.NormalizeBrand(value);
        if (brand is null)
        {
            AddIssue(issues, ProductCatalogResetIssueLevel.BlockingError, row, $"Marka değeri mevcut ProductBrand yapısıyla eşleşmiyor: {DisplayRaw(value)}");
        }
        return brand ?? ProductBrand.NgSeramik;
    }

    private ProductStatus ParseStatus(string? value, ProductImportRow row, List<ProductCatalogResetIssueDto> issues)
    {
        var status = _normalizer.NormalizeStatus(value);
        if (status is null)
        {
            AddIssue(issues, ProductCatalogResetIssueLevel.RowWarning, row, $"Durum değeri okunamadığı için Devam olarak işlendi: {DisplayRaw(value)}");
        }
        return status ?? ProductStatus.InProgress;
    }

    private string? OptionalText(string? value, ProductImportRow row, List<ProductCatalogResetIssueDto> issues, string warning)
    {
        var text = _normalizer.NullIfBlankOrDash(value);
        if (text is null)
        {
            AddIssue(issues, ProductCatalogResetIssueLevel.RowWarning, row, warning);
        }
        return text;
    }

    private string? OptionalArea(string? value, ProductImportRow row, List<ProductCatalogResetIssueDto> issues, string warning, Func<string?, string?> normalize)
    {
        if (_normalizer.NullIfBlankOrDash(value) is null)
        {
            AddIssue(issues, ProductCatalogResetIssueLevel.RowWarning, row, warning);
            return null;
        }
        return normalize(value);
    }

    private int? ParseFaceCount(string? value, ProductImportRow row, List<ProductCatalogResetIssueDto> issues)
    {
        var text = _normalizer.NullIfBlankOrDash(value);
        if (text is null)
        {
            return null;
        }
        if (IsYok(text))
        {
            AddIssue(issues, ProductCatalogResetIssueLevel.RowWarning, row, "Face sayısı ‘yok’ olarak belirtildiği için boş bırakıldı.");
            return null;
        }
        var parsed = _normalizer.NormalizeInt(text);
        if (parsed is null || parsed < 0)
        {
            AddIssue(issues, ProductCatalogResetIssueLevel.BlockingError, row, $"Face Sayısı değeri tam sayı olarak okunamadı: {DisplayRaw(value)}");
        }
        return parsed;
    }

    private bool? ParseGlazedGranite(string? value, ProductImportRow row, List<ProductCatalogResetIssueDto> issues)
    {
        var text = _normalizer.NullIfBlankOrDash(value);
        if (text is null)
        {
            return null;
        }
        if (ReferenceKey(text) == ReferenceKey("HAYIR (SIRSIZ)"))
        {
            AddIssue(issues, ProductCatalogResetIssueLevel.RowWarning, row, "Sırlı Granite değeri ‘HAYIR (SIRSIZ)’ ifadesinden false olarak normalize edildi.");
            return false;
        }
        var parsed = _normalizer.NormalizeBoolean(text);
        if (parsed is null)
        {
            AddIssue(issues, ProductCatalogResetIssueLevel.BlockingError, row, $"Sırlı Granite değeri geçersiz: {DisplayRaw(value)}");
        }
        return parsed;
    }

    private decimal? ParseNullableDecimal(string? value, ProductImportRow row, List<ProductCatalogResetIssueDto> issues, string label)
    {
        if (_normalizer.NullIfBlankOrDash(value) is null)
        {
            return null;
        }
        var parsed = _normalizer.NormalizeDecimal(value);
        if (parsed is null || parsed < 0 || (label == "Kalınlık" && parsed <= 0))
        {
            AddIssue(issues, ProductCatalogResetIssueLevel.BlockingError, row, $"{label} değeri sayı olarak okunamadı veya geçersiz: {DisplayRaw(value)}");
        }
        return parsed;
    }

    private decimal? ParsePEI(string? value, ProductImportRow row, List<ProductCatalogResetIssueDto> issues)
    {
        if (_normalizer.NullIfBlankOrDash(value) is null)
        {
            return null;
        }
        var parsed = _normalizer.NormalizePEI(value);
        if (parsed is not (>= 1 and <= 5))
        {
            AddIssue(issues, ProductCatalogResetIssueLevel.BlockingError, row, $"PEI Değeri 1 ile 5 arasında olmalıdır: {DisplayRaw(value)}");
        }
        return parsed;
    }

    private string? ParseRValue(string? value, ProductImportRow row, List<ProductCatalogResetIssueDto> issues)
    {
        if (_normalizer.NullIfBlankOrDash(value) is null)
        {
            return null;
        }
        var parsed = _normalizer.NormalizeRValue(value);
        if (parsed is null)
        {
            AddIssue(issues, ProductCatalogResetIssueLevel.BlockingError, row, $"R Değeri geçersiz: {DisplayRaw(value)}");
        }
        return parsed;
    }

    private (bool? AntiSlip, string? RValue) ParseAntiSlip(string? value, string? explicitRValue, ProductImportRow row, List<ProductCatalogResetIssueDto> issues)
    {
        var parsed = _normalizer.NormalizeAntiSlip(value);
        if (!parsed.Recognized)
        {
            AddIssue(issues, ProductCatalogResetIssueLevel.BlockingError, row, $"Kaymazlık değeri geçersiz: {DisplayRaw(value)}");
        }
        if (explicitRValue is not null && parsed.RValue is not null && !string.Equals(explicitRValue, parsed.RValue, StringComparison.OrdinalIgnoreCase))
        {
            AddIssue(issues, ProductCatalogResetIssueLevel.BlockingError, row, $"Kaymazlık ve R Değeri çelişkili: Kaymazlık={DisplayRaw(value)}, R Değeri={explicitRValue}");
        }
        return (parsed.AntiSlip, parsed.RValue);
    }

    private bool? ParseBoolean(string? value, ProductImportRow row, List<ProductCatalogResetIssueDto> issues, string label)
    {
        if (_normalizer.NullIfBlankOrDash(value) is null)
        {
            return null;
        }
        if (_normalizer.TryParseBoolean(value, out var result))
        {
            return result;
        }
        AddIssue(issues, ProductCatalogResetIssueLevel.BlockingError, row, $"{label} değeri geçersiz: {DisplayRaw(value)}");
        return null;
    }

    private async Task<object> BuildBackupAsync() => new
    {
        Products = await _dbContext.Products.AsNoTracking().ToListAsync(),
        ProductImages = await _dbContext.ProductImages.AsNoTracking().ToListAsync(),
        ProductDocuments = await _dbContext.ProductDocuments.AsNoTracking().ToListAsync(),
        ProductReferenceProjects = await _dbContext.ProductReferenceProjects.AsNoTracking().ToListAsync(),
        CollectionDocuments = await _dbContext.CollectionDocuments.AsNoTracking().ToListAsync(),
        Categories = await _dbContext.Categories.AsNoTracking().ToListAsync(),
        Collections = await _dbContext.Collections.AsNoTracking().ToListAsync(),
        Translations = await _dbContext.Translations.AsNoTracking()
            .Where(t => t.EntityType == EntityType.Product || t.EntityType == EntityType.Category || t.EntityType == EntityType.Collection)
            .ToListAsync()
    };

    private async Task ValidateFinalStateAsync(int productCount, int categoryCount, int collectionCount)
    {
        if (await _dbContext.Products.CountAsync() != productCount ||
            await _dbContext.Categories.CountAsync() != categoryCount ||
            await _dbContext.Collections.CountAsync() != collectionCount ||
            await _dbContext.Products.AnyAsync(p => p.CategoryId == 0 || p.CollectionId == 0))
        {
            throw new InvalidOperationException("Son doğrulama sayıları beklenen değerlerle eşleşmedi.");
        }
    }

    private async Task<string> SaveJsonAsync(string folder, string fileName, object value)
    {
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return await _fileStorageService.SaveAsync(folder, stream, fileName);
    }

    private static void AddIssue(List<ProductCatalogResetIssueDto> issues, ProductCatalogResetIssueLevel level, ProductImportRow row, string message) =>
        issues.Add(new ProductCatalogResetIssueDto { Level = level, RowNumber = row.RowNumber, ProductCode = row.ProductCode, Message = message });

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static async Task<string> ComputeHashAsync(Stream stream)
    {
        stream.Position = 0;
        var hash = await SHA256.HashDataAsync(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ReferenceKey(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToUpper(new System.Globalization.CultureInfo("tr-TR"));

    private static bool IsYok(string value) => ReferenceKey(value) is "YOK";

    private static string DisplayRaw(string? value) => string.IsNullOrWhiteSpace(value) ? "boş" : value.Trim();

    private static string ResolveBatchId(string sourceFilePath)
    {
        var parts = sourceFilePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var productsIndex = Array.FindIndex(parts, p => string.Equals(p, "products", StringComparison.OrdinalIgnoreCase));
        return productsIndex >= 0 && productsIndex + 1 < parts.Length ? parts[productsIndex + 1] : Guid.NewGuid().ToString("N");
    }

    private static ProductCatalogResetResultDto Failed(string batchId, string hash, DateTime startedAt, string requestedBy, string error) =>
        new()
        {
            BatchId = batchId,
            FileHash = hash,
            StartedAtUtc = startedAt,
            FinishedAtUtc = DateTime.UtcNow,
            RequestedBy = requestedBy,
            Succeeded = false,
            Errors = [error]
        };

    private static ProductCatalogResetResultDto WithResultPath(ProductCatalogResetResultDto result, string resultPath) =>
        result with { ResultPath = resultPath };

    private sealed record ResetRow(
        int RowNumber,
        string ProductCode,
        string ProductName,
        string SeriesName,
        string SeriesKey,
        string? RawBrand,
        ProductBrand Brand,
        string? CategoryName,
        string CategoryKey,
        ProductStatus Status,
        string? CommercialName,
        string? ProductGroup,
        string Size,
        string Unit,
        string? Surface,
        string? Relief,
        string? SpecialSurface,
        int? FaceCount,
        decimal? Thickness,
        string? BodyType,
        string? Color,
        string? ColorMaterial,
        string? ApplicationArea,
        string? UsageArea,
        string? Finish,
        decimal? PEI,
        string? VValue,
        string? RValue,
        string? DeepAbrasion,
        bool? HeatResistance,
        bool? AntiSlip,
        bool? GlazedGranite,
        bool? HasFace,
        bool? HasVenue,
        decimal? BoxM2,
        decimal? PalletM2,
        IReadOnlyList<ProductCatalogResetIssueDto> Issues);
}
