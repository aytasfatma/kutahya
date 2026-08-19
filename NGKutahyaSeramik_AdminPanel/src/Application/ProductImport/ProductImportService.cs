using System.Text;
using System.Text.Json;
using Application.Categories;
using Application.Collections;
using Application.Products;
using Application.Storage;
using Application.Surfaces;
using Application.Translations;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.ProductImport;

public class ProductImportService
{
    private const string TrLanguageCode = "TR";
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private static readonly byte[] XlsxSignature = [0x50, 0x4B, 0x03, 0x04];
    private static readonly IReadOnlySet<string> RequiredHeaders = new HashSet<string>(
        [ProductImportHeaders.ProductCode, ProductImportHeaders.ProductName, ProductImportHeaders.Series,
         ProductImportHeaders.Status, ProductImportHeaders.Size],
        StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> ValidUnits = new HashSet<string>(["M2", "ADT"], StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> ValidBodyTypes = new HashSet<string>(["Sırlı Porselen", "Duvar Karosu", "Teknik Granit Seramik"], StringComparer.CurrentCultureIgnoreCase);
    private static readonly IReadOnlySet<string> ValidVValues = new HashSet<string>(["V1", "V2", "V3", "V4"], StringComparer.OrdinalIgnoreCase);

    public static readonly IReadOnlyList<string> ExpectedHeaders = ProductImportHeaders.CanonicalHeaders;

    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICollectionRepository _collectionRepository;
    private readonly ITranslationService _translationService;
    private readonly IProductImportFileReader _fileReader;
    private readonly IProductImportTemplateWriter _templateWriter;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductImportService> _logger;
    private readonly ISurfaceRepository? _surfaceRepository;
    private readonly ProductImportNormalizer _normalizer = new();

    public ProductImportService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ICollectionRepository collectionRepository,
        ITranslationService translationService,
        IProductImportFileReader fileReader,
        IProductImportTemplateWriter templateWriter,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<ProductImportService> logger,
        ISurfaceRepository? surfaceRepository = null)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _collectionRepository = collectionRepository;
        _translationService = translationService;
        _fileReader = fileReader;
        _templateWriter = templateWriter;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _surfaceRepository = surfaceRepository;
    }

    public string? ValidateUploadedFile(string originalFileName, string contentType, long length, Stream content)
    {
        if (length <= 0)
        {
            return "Boş dosya yüklenemez.";
        }

        if (length > MaxFileSizeBytes)
        {
            return $"Dosya boyutu {MaxFileSizeBytes / (1024 * 1024)} MB sınırını aşıyor.";
        }

        if (!string.Equals(Path.GetExtension(originalFileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return "Yalnızca .xlsx dosyası yüklenebilir.";
        }

        if (!content.CanSeek)
        {
            return "Dosya akışı okunamadı.";
        }

        var header = new byte[4];
        content.Position = 0;
        var bytesRead = content.Read(header, 0, header.Length);
        content.Position = 0;

        return bytesRead < 4 || !header.AsSpan(0, 4).SequenceEqual(XlsxSignature)
            ? "Dosya içeriği geçerli bir .xlsx dosyası gibi görünmüyor."
            : null;
    }

    public async Task<ProductImportParseResultDto> PreviewAsync(Stream fileStream)
    {
        var (headerValid, headerErrors, rows) = await ParseAndValidateAsync(fileStream);
        return new ProductImportParseResultDto
        {
            HeaderValid = headerValid,
            HeaderErrors = headerErrors,
            Rows = rows
        };
    }

    public async Task<ProductImportResultDto> ImportAsync(Stream fileStream)
    {
        var (headerValid, headerErrors, rows) = await ParseAndValidateAsync(fileStream);
        if (!headerValid)
        {
            return new ProductImportResultDto
            {
                Total = rows.Count,
                Failed = 1,
                RowResults = HeaderFailureRows(headerErrors)
            };
        }

        var created = 0;
        var updated = 0;
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var trLanguage = (await _translationService.GetActiveLanguagesAsync()).First(l => l.Code == TrLanguageCode);
            var allProducts = await _productRepository.GetAllAsync();
            var nextDisplayOrder = allProducts.Count == 0 ? 1 : allProducts.Max(p => p.DisplayOrder) + 1;

            foreach (var row in rows.Where(r => r.IsValid))
            {
                var surface = await ResolveSurfaceAsync(row.ResolvedSurface);
                if (row.Action == ProductImportRowAction.New)
                {
                    var product = new Product(
                        row.ProductCode, row.ResolvedCategoryId, row.ResolvedCollectionId,
                        row.ResolvedBrand, row.ResolvedStatus,
                        row.ResolvedCommercialName, row.ResolvedProductGroup,
                        row.ResolvedSize, row.ResolvedUnit, row.ResolvedSurface,
                        row.ResolvedRelief, row.ResolvedSpecialSurface, row.ResolvedFaceCount,
                        row.ResolvedThickness, row.ResolvedBodyType, row.ResolvedColor, row.ResolvedColorMaterial,
                        row.ResolvedApplicationArea, row.ResolvedUsageArea,
                        row.ResolvedFinish, row.ResolvedPEI, row.ResolvedVValue, row.ResolvedRValue, row.ResolvedDeepAbrasion,
                        row.ResolvedHeatResistance, row.ResolvedAntiSlip, row.ResolvedGlazedGranite,
                        row.ResolvedHasFace, row.ResolvedHasVenue, row.ResolvedBoxM2, row.ResolvedPalletM2,
                        nextDisplayOrder++);

                    product.SetSurface(surface?.Id, row.ResolvedSurface);

                    await _productRepository.AddAsync(product);
                    await _unitOfWork.SaveChangesAsync();
                    await _translationService.SetTranslationAsync(EntityType.Product, product.Id, trLanguage.Id, ProductFields.Name, row.ProductName!.Trim());
                    await _translationService.SetTranslationAsync(EntityType.Product, product.Id, trLanguage.Id, ProductFields.SeoUrl, product.ProductCode.ToLowerInvariant());
                    created++;
                }
                else
                {
                    var existing = await _productRepository.GetByIdAsync(row.ExistingProductId!.Value)
                        ?? throw new InvalidOperationException($"Satır {row.RowNumber}: ürün bulunamadı.");

                    existing.UpdateDetails(
                        existing.ProductCode, row.ResolvedCategoryId, row.ResolvedCollectionId,
                        row.ResolvedBrand, row.ResolvedStatus,
                        row.ResolvedCommercialName ?? existing.CommercialName,
                        row.ResolvedProductGroup ?? existing.ProductGroup,
                        row.ResolvedSize, row.ResolvedUnit, row.ResolvedSurface,
                        row.ResolvedRelief ?? existing.Relief, row.ResolvedSpecialSurface ?? existing.SpecialSurface,
                        row.ResolvedFaceCount ?? existing.FaceCount, row.ResolvedThickness, row.ResolvedBodyType,
                        row.ResolvedColor, row.ResolvedColorMaterial ?? existing.ColorMaterial,
                        row.ResolvedApplicationArea ?? existing.ApplicationArea,
                        row.ResolvedUsageArea ?? existing.UsageArea,
                        row.ResolvedFinish ?? existing.Finish, row.ResolvedPEI ?? existing.PEI,
                        row.ResolvedVValue ?? existing.VValue, row.ResolvedRValue ?? existing.RValue,
                        row.ResolvedDeepAbrasion ?? existing.DeepAbrasion,
                        row.ResolvedHeatResistance ?? existing.HeatResistance,
                        row.ResolvedAntiSlip ?? existing.AntiSlip,
                        row.ResolvedGlazedGranite ?? existing.GlazedGranite,
                        row.ResolvedHasFace ?? existing.HasFace,
                        row.ResolvedHasVenue ?? existing.HasVenue,
                        row.ResolvedBoxM2 ?? existing.BoxM2, row.ResolvedPalletM2 ?? existing.PalletM2,
                        existing.DisplayOrder);

                    existing.SetSurface(surface?.Id, row.ResolvedSurface);

                    await _translationService.SetTranslationAsync(EntityType.Product, existing.Id, trLanguage.Id, ProductFields.Name, row.ProductName!.Trim());
                    await _translationService.SetTranslationAsync(EntityType.Product, existing.Id, trLanguage.Id, ProductFields.SeoUrl, existing.ProductCode.ToLowerInvariant());
                    updated++;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Ürün Excel import sırasında hata oluştu; transaction geri alındı.");
            return new ProductImportResultDto
            {
                Total = rows.Count,
                Failed = rows.Count,
                RowResults = rows,
                TransactionRolledBack = true
            };
        }

        return new ProductImportResultDto
        {
            Total = rows.Count,
            Succeeded = created + updated,
            Created = created,
            Updated = updated,
            Failed = rows.Count(r => !r.IsValid),
            Skipped = rows.Count(r => r.IsValid && r.Action == ProductImportRowAction.Skipped),
            RowResults = rows
        };
    }

    private async Task<Surface?> ResolveSurfaceAsync(string? name)
    {
        if (_surfaceRepository is null || string.IsNullOrWhiteSpace(name)) return null;
        var normalized = name.Trim();
        var existing = await _surfaceRepository.GetByNameAsync(normalized);
        if (existing is not null) return existing;
        var nextOrder = (await _surfaceRepository.GetAllAsync()).Select(x => x.DisplayOrder).DefaultIfEmpty(0).Max() + 1;
        var created = new Surface(normalized, nextOrder);
        await _surfaceRepository.AddAsync(created);
        await _unitOfWork.SaveChangesAsync();
        return created;
    }

    public byte[] GenerateTemplate() => _templateWriter.GenerateTemplate();

    public Task<string> SaveTempFileAsync(Stream content)
    {
        var batchId = Guid.NewGuid().ToString("N");
        return _fileStorageService.SaveAsync($"imports/products/{batchId}", content, "source.xlsx");
    }

    public Task<Stream> OpenTempFileAsync(string relativeFilePath) => _fileStorageService.OpenReadAsync(relativeFilePath);

    public void DeleteTempFile(string relativeFilePath)
    {
        // Source Excel kalıcı batch kaydıdır; confirm sonrası silinmez.
    }

    public async Task<string> SaveReportAsync(ProductImportReportDto report, string? sourceFilePath = null)
    {
        var json = JsonSerializer.Serialize(report);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return await _fileStorageService.SaveAsync(ResolveReportFolder(sourceFilePath), stream, "result.json");
    }

    public async Task<ProductImportReportDto> LoadReportAsync(string relativeFilePath)
    {
        await using var stream = await _fileStorageService.OpenReadAsync(relativeFilePath);
        var report = await JsonSerializer.DeserializeAsync<ProductImportReportDto>(stream);
        return report ?? throw new InvalidOperationException("Rapor dosyası okunamadı.");
    }

    private async Task<(bool HeaderValid, IReadOnlyList<string> HeaderErrors, List<ProductImportRowResult> Rows)> ParseAndValidateAsync(Stream fileStream)
    {
        var (headers, rawRows) = _fileReader.Read(fileStream);
        var recognizedHeaders = headers.Select(ProductImportHeaders.ResolveCanonical).OfType<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var headerErrors = headers
            .Where(header => ProductImportHeaders.ResolveCanonical(header) is null)
            .Select(header => $"Tanınmayan sütun: {header}")
            .ToList();

        var missingHeaders = RequiredHeaders.Where(required => !recognizedHeaders.Contains(required)).ToList();
        if (missingHeaders.Count > 0)
        {
            headerErrors.Add($"Eksik zorunlu sütun(lar): {string.Join(", ", missingHeaders.Select(ProductImportHeaders.DisplayName))}");
        }

        if (headerErrors.Count > 0)
        {
            return (false, headerErrors, []);
        }

        var (categoryNamesById, collectionNamesById) = await LoadReferenceNamesAsync();
        var productCodeOccurrences = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
        var results = new List<ProductImportRowResult>();

        foreach (var row in rawRows)
        {
            var result = await ValidateRowAsync(row, recognizedHeaders, categoryNamesById, collectionNamesById, productCodeOccurrences);
            results.Add(result);
        }

        return (true, [], results);
    }

    private async Task<ProductImportRowResult> ValidateRowAsync(
        ProductImportRow row,
        IReadOnlySet<string> recognizedHeaders,
        IReadOnlyDictionary<int, string> categoryNamesById,
        IReadOnlyDictionary<int, string> collectionNamesById,
        Dictionary<string, int> productCodeOccurrences)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var sourceProductCode = _normalizer.NormalizeWhitespace(row.ProductCode) ?? string.Empty;
        var productCode = sourceProductCode;
        if (string.IsNullOrWhiteSpace(productCode))
        {
            errors.Add("Ürün Kodu zorunludur.");
        }
        else
        {
            productCodeOccurrences.TryGetValue(sourceProductCode, out var occurrence);
            occurrence++;
            productCodeOccurrences[sourceProductCode] = occurrence;

            if (occurrence > 1)
            {
                productCode = $"{sourceProductCode}-{occurrence}";
                warnings.Add(
                    $"Aynı kaynak ürün kodunun {occurrence}. varyantı olduğu için kayıt kodu {productCode} olarak oluşturuldu.");
            }
        }

        var productName = _normalizer.NormalizeWhitespace(row.ProductName);
        if (string.IsNullOrWhiteSpace(productName))
        {
            errors.Add("Ürün Adı zorunludur.");
        }

        var existingProduct = string.IsNullOrWhiteSpace(productCode) ? null : await _productRepository.GetByProductCodeAsync(productCode);
        var action = existingProduct is null ? ProductImportRowAction.New : ProductImportRowAction.Update;

        var (collectionId, collectionName) = ResolveCollection(row.Series, collectionNamesById, errors);
        var (categoryId, categoryName, isUncategorizedCategory) = ResolveCategory(row.Category, categoryNamesById, errors, warnings);

        var status = RequireStatus(row.Status, errors);
        var brand = recognizedHeaders.Contains(ProductImportHeaders.Brand)
            ? RequireBrand(row.Brand, errors)
            : existingProduct?.Brand ?? ProductBrand.NgSeramik;
        var size = RequireText(ProductImportHeaders.Size, row.Size, errors, _normalizer.NormalizeSize);
        var unit = recognizedHeaders.Contains(ProductImportHeaders.Unit)
            ? RequireUnit(row.Unit, errors)
            : existingProduct?.Unit ?? "M2";
        var surface = OptionalText(row.Surface, warnings, "Yüzey bilgisi bulunmadığı için boş bırakıldı.");
        var color = OptionalText(row.Color, warnings, "Renk bilgisi bulunmadığı için boş bırakıldı.");
        var thickness = recognizedHeaders.Contains(ProductImportHeaders.Thickness)
            ? ParseOptionalPositiveDecimal(ProductImportHeaders.Thickness, row.Thickness, errors)
            : existingProduct?.Thickness;
        var bodyType = recognizedHeaders.Contains(ProductImportHeaders.BodyType)
            ? OptionalBodyType(row.BodyType, errors)
            : existingProduct?.BodyType;
        var faceCount = ParseFaceCount(row.FaceCount, errors, warnings) ?? existingProduct?.FaceCount;
        var pei = ParsePEI(row.PEI, errors);
        var vValue = ParseVValue(row.VValue, errors);
        var rValue = ParseRValue(row.RValue, errors);
        var antiSlip = ParseAntiSlip(row.AntiSlip, rValue, errors);
        var boxM2 = ParseNonNegativeDecimal(ProductImportHeaders.BoxM2, row.BoxM2, errors);
        var palletM2 = ParseNonNegativeDecimal(ProductImportHeaders.PalletM2, row.PalletM2, errors);

        var isValid = errors.Count == 0;
        return new ProductImportRowResult
        {
            RowNumber = row.RowNumber,
            ProductCode = productCode,
            ProductName = productName,
            IsValid = isValid,
            Action = isValid ? action : ProductImportRowAction.Skipped,
            Errors = errors,
            Warnings = warnings,
            ExistingProductId = existingProduct?.Id,
            ResolvedCategoryId = categoryId,
            ResolvedCategoryName = categoryName,
            IsUncategorizedCategory = isUncategorizedCategory,
            ResolvedCollectionId = collectionId,
            ResolvedCollectionName = collectionName,
            ResolvedBrand = brand,
            ResolvedStatus = status,
            ResolvedCommercialName = _normalizer.NullIfBlankOrDash(row.CommercialName),
            ResolvedProductGroup = _normalizer.NullIfBlankOrDash(row.ProductGroup),
            ResolvedSize = size,
            ResolvedUnit = unit,
            ResolvedSurface = surface,
            ResolvedRelief = _normalizer.NullIfBlankOrDash(row.Relief),
            ResolvedSpecialSurface = _normalizer.NullIfBlankOrDash(row.SpecialSurface),
            ResolvedColor = color,
            ResolvedColorMaterial = _normalizer.NullIfBlankOrDash(row.ColorMaterial),
            ResolvedUsageArea = OptionalArea(row.UsageArea, warnings, "Kullanım alanı bilgisi bulunmadığı için boş bırakıldı.", _normalizer.NormalizeUsageArea),
            ResolvedApplicationArea = OptionalArea(row.ApplicationArea, warnings, "Uygulama alanı bilgisi bulunmadığı için boş bırakıldı.", _normalizer.NormalizeApplicationArea),
            ResolvedFaceCount = faceCount,
            ResolvedThickness = thickness,
            ResolvedBodyType = bodyType,
            ResolvedVValue = vValue,
            ResolvedPEI = pei,
            ResolvedRValue = rValue ?? antiSlip.RValue,
            ResolvedDeepAbrasion = _normalizer.NullIfBlankOrDash(row.DeepAbrasion),
            ResolvedFinish = _normalizer.NullIfBlankOrDash(row.Finish),
            ResolvedHeatResistance = ParseBoolean(ProductImportHeaders.HeatResistance, row.HeatResistance, errors),
            ResolvedAntiSlip = antiSlip.AntiSlip,
            ResolvedGlazedGranite = ParseGlazedGranite(row.GlazedGranite, errors, warnings),
            ResolvedHasFace = ParseBoolean(ProductImportHeaders.HasFace, row.HasFace, errors),
            ResolvedHasVenue = ParseBoolean(ProductImportHeaders.HasVenue, row.HasVenue, errors),
            ResolvedBoxM2 = boxM2,
            ResolvedPalletM2 = palletM2
        };
    }

    private async Task<(Dictionary<int, string> Categories, Dictionary<int, string> Collections)> LoadReferenceNamesAsync()
    {
        var categories = new Dictionary<int, string>();
        foreach (var category in await _categoryRepository.GetAllAsync())
        {
            var name = (await _translationService.GetTranslationsAsync(EntityType.Category, category.Id))
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == CategoryFields.Name)?.Value;
            if (!string.IsNullOrWhiteSpace(name))
            {
                categories[category.Id] = name;
            }
        }

        var collections = new Dictionary<int, string>();
        foreach (var collection in await _collectionRepository.GetAllAsync())
        {
            var name = (await _translationService.GetTranslationsAsync(EntityType.Collection, collection.Id))
                .FirstOrDefault(t => t.LanguageCode == TrLanguageCode && t.FieldName == CollectionFields.Name)?.Value;
            if (!string.IsNullOrWhiteSpace(name))
            {
                collections[collection.Id] = name;
            }
        }

        return (categories, collections);
    }

    private (int Id, string? Name) ResolveCollection(string? value, IReadOnlyDictionary<int, string> names, List<string> errors)
    {
        var text = _normalizer.NormalizeWhitespace(value);
        var match = names.FirstOrDefault(kv => _normalizer.ReferenceEquals(kv.Value, text));
        if (string.IsNullOrWhiteSpace(text) || match.Value is null)
        {
            errors.Add($"Eşleşmeyen seri: {DisplayRaw(value)}");
            return (0, null);
        }
        return (match.Key, match.Value);
    }

    private (int? Id, string? Name, bool IsUncategorized) ResolveCategory(string? value, IReadOnlyDictionary<int, string> names, List<string> errors, List<string> warnings)
    {
        var text = _normalizer.NormalizeWhitespace(value);
        if (string.IsNullOrWhiteSpace(text) || text == "-")
        {
            warnings.Add("Excel’de kategori bilgisi bulunmadığı için ürün kategori seçilmeden aktarılacaktır.");
            return (null, null, true);
        }

        var match = names.FirstOrDefault(kv => _normalizer.ReferenceEquals(kv.Value, text));
        if (string.IsNullOrWhiteSpace(text) || match.Value is null)
        {
            errors.Add($"Eşleşmeyen kategori: {DisplayRaw(value)}");
            return (null, null, false);
        }
        return (match.Key, match.Value, false);
    }

    private ProductStatus RequireStatus(string? value, List<string> errors) =>
        _normalizer.NormalizeStatus(value) ?? AddError(ProductStatus.Active, errors, $"Durum değeri geçersiz: {DisplayRaw(value)}");

    private ProductBrand RequireBrand(string? value, List<string> errors) =>
        _normalizer.NormalizeBrand(value) ?? AddError(ProductBrand.NgSeramik, errors, $"Ürün Segmentasyonu değeri geçersiz: {DisplayRaw(value)}");

    private string RequireUnit(string? value, List<string> errors)
    {
        var unit = _normalizer.NormalizeUnit(value);
        return unit is not null && ValidUnits.Contains(unit)
            ? unit
            : AddError("M2", errors, $"Birim değeri geçersiz: {DisplayRaw(value)}");
    }

    private string RequireBodyType(string? value, List<string> errors)
    {
        var bodyType = _normalizer.NullIfBlankOrDash(value);
        var canonical = bodyType is null
            ? null
            : ValidBodyTypes.FirstOrDefault(valid => _normalizer.ReferenceEquals(valid, bodyType));
        return canonical is not null
            ? canonical
            : AddError("Sırlı Porselen", errors, $"Bünye değeri geçersiz: {DisplayRaw(value)}");
    }

    private string RequireText(string header, string? value, List<string> errors, Func<string?, string?> normalize)
    {
        var normalized = normalize(value);
        return string.IsNullOrWhiteSpace(normalized)
            ? AddError(string.Empty, errors, $"{ProductImportHeaders.DisplayName(header)} zorunludur.")
            : normalized;
    }

    private string? OptionalText(string? value, List<string> warnings, string warning)
    {
        var normalized = _normalizer.NullIfBlankOrDash(value);
        if (normalized is null)
        {
            warnings.Add(warning);
        }
        return normalized;
    }

    private string? OptionalArea(string? value, List<string> warnings, string warning, Func<string?, string?> normalize)
    {
        if (_normalizer.NullIfBlankOrDash(value) is null)
        {
            warnings.Add(warning);
            return null;
        }
        return normalize(value);
    }

    private string? OptionalBodyType(string? value, List<string> errors)
    {
        var bodyType = _normalizer.NullIfBlankOrDash(value);
        if (bodyType is null)
        {
            return null;
        }

        var canonical = ValidBodyTypes.FirstOrDefault(valid => _normalizer.ReferenceEquals(valid, bodyType));
        return canonical is not null
            ? canonical
            : AddError<string?>(null, errors, $"Bünye değeri geçersiz: {DisplayRaw(value)}");
    }

    private decimal? ParseOptionalPositiveDecimal(string header, string? value, List<string> errors)
    {
        if (_normalizer.NullIfBlankOrDash(value) is null)
        {
            return null;
        }

        var parsed = _normalizer.NormalizeDecimal(value);
        return parsed is > 0
            ? parsed.Value
            : AddError<decimal?>(null, errors, $"{ProductImportHeaders.DisplayName(header)} değeri sayı olarak okunamadı veya sıfırdan büyük değil: {DisplayRaw(value)}");
    }

    private int? ParseFaceCount(string? value, List<string> errors, List<string> warnings)
    {
        var normalized = _normalizer.NullIfBlankOrDash(value);
        if (normalized is null)
        {
            return null;
        }

        if (_normalizer.ReferenceEquals(normalized, "Yok"))
        {
            warnings.Add("Face sayısı ‘yok’ olarak belirtildiği için boş bırakıldı.");
            return null;
        }

        var parsed = _normalizer.NormalizeInt(value);
        return parsed is >= 0 ? parsed : AddError<int?>(null, errors, $"{ProductImportHeaders.DisplayName(ProductImportHeaders.FaceCount)} değeri pozitif tam sayı olmalıdır: {DisplayRaw(value)}");
    }

    private decimal RequirePositiveDecimal(string header, string? value, List<string> errors)
    {
        var parsed = _normalizer.NormalizeDecimal(value);
        return parsed is > 0
            ? parsed.Value
            : AddError(1m, errors, $"{ProductImportHeaders.DisplayName(header)} değeri sayı olarak okunamadı veya sıfırdan büyük değil: {DisplayRaw(value)}");
    }

    private int? ParseNonNegativeInt(string header, string? value, List<string> errors)
    {
        if (_normalizer.NullIfBlankOrDash(value) is null)
        {
            return null;
        }
        var parsed = _normalizer.NormalizeInt(value);
        return parsed is >= 0 ? parsed : AddError<int?>(null, errors, $"{ProductImportHeaders.DisplayName(header)} değeri pozitif tam sayı olmalıdır: {DisplayRaw(value)}");
    }

    private decimal? ParseNonNegativeDecimal(string header, string? value, List<string> errors)
    {
        if (_normalizer.NullIfBlankOrDash(value) is null)
        {
            return null;
        }
        var parsed = _normalizer.NormalizeDecimal(value);
        return parsed is >= 0 ? parsed : AddError<decimal?>(null, errors, $"{ProductImportHeaders.DisplayName(header)} değeri sayı olarak okunamadı veya negatif: {DisplayRaw(value)}");
    }

    private decimal? ParsePEI(string? value, List<string> errors)
    {
        if (_normalizer.NullIfBlankOrDash(value) is null)
        {
            return null;
        }
        var parsed = _normalizer.NormalizePEI(value);
        return parsed is >= 1 and <= 5 ? parsed : AddError<decimal?>(null, errors, $"PEI Değeri 1 ile 5 arasında olmalıdır: {DisplayRaw(value)}");
    }

    private string? ParseVValue(string? value, List<string> errors)
    {
        var parsed = _normalizer.NormalizeVValue(value);
        return parsed is null || ValidVValues.Contains(parsed) ? parsed : AddError<string?>(null, errors, $"V Değeri geçersiz: {DisplayRaw(value)}");
    }

    private string? ParseRValue(string? value, List<string> errors)
    {
        var parsed = _normalizer.NormalizeRValue(value);
        return parsed is not null || _normalizer.NullIfBlankOrDash(value) is null ? parsed : AddError<string?>(null, errors, $"R Değeri geçersiz: {DisplayRaw(value)}");
    }

    private (bool? AntiSlip, string? RValue) ParseAntiSlip(string? value, string? explicitRValue, List<string> errors)
    {
        var parsed = _normalizer.NormalizeAntiSlip(value);
        if (!parsed.Recognized)
        {
            errors.Add($"Kaymazlık değeri geçersiz: {DisplayRaw(value)}");
        }
        if (explicitRValue is not null && parsed.RValue is not null && !string.Equals(explicitRValue, parsed.RValue, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add($"Kaymazlık ve R Değeri çelişkili: Kaymazlık={DisplayRaw(value)}, R Değeri={explicitRValue}");
        }
        return (parsed.AntiSlip, parsed.RValue);
    }

    private bool? ParseBoolean(string header, string? value, List<string> errors)
    {
        return _normalizer.TryParseBoolean(value, out var result)
            ? result
            : AddError<bool?>(null, errors, $"{ProductImportHeaders.DisplayName(header)} değeri geçersiz: {DisplayRaw(value)}");
    }

    private bool? ParseGlazedGranite(string? value, List<string> errors, List<string> warnings)
    {
        var normalized = _normalizer.NullIfBlankOrDash(value);
        if (normalized is null)
        {
            return null;
        }

        if (_normalizer.ReferenceEquals(normalized, "HAYIR (SIRSIZ)"))
        {
            warnings.Add("Sırlı Granite değeri ‘HAYIR (SIRSIZ)’ ifadesinden false olarak normalize edildi.");
            return false;
        }

        return _normalizer.TryParseBoolean(value, out var result)
            ? result
            : AddError<bool?>(null, errors, $"{ProductImportHeaders.DisplayName(ProductImportHeaders.GlazedGranite)} değeri geçersiz: {DisplayRaw(value)}");
    }

    private static T AddError<T>(T fallback, List<string> errors, string error)
    {
        errors.Add(error);
        return fallback;
    }

    private static string ResolveReportFolder(string? sourceFilePath)
    {
        if (sourceFilePath is not null && sourceFilePath.Contains("/imports/products/", StringComparison.OrdinalIgnoreCase))
        {
            var trimmed = sourceFilePath.Trim('/');
            var folder = Path.GetDirectoryName(trimmed)?.Replace('\\', '/');
            if (folder is not null && folder.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
            {
                folder = folder["uploads/".Length..];
            }
            return folder ?? "imports/products/reports";
        }
        return $"imports/products/{Guid.NewGuid():N}";
    }

    private static List<ProductImportRowResult> HeaderFailureRows(IReadOnlyList<string> headerErrors) =>
    [
        new ProductImportRowResult
        {
            RowNumber = 1,
            ProductCode = string.Empty,
            IsValid = false,
            Action = ProductImportRowAction.Skipped,
            Errors = headerErrors
        }
    ];

    private static string DisplayRaw(string? value) => string.IsNullOrWhiteSpace(value) ? "boş" : value.Trim();
}


