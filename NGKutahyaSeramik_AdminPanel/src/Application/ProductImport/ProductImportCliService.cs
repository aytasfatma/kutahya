using Application.Categories;
using Application.Collections;
using Domain.Enums;

namespace Application.ProductImport;

public sealed class ProductImportCliService
{
    private readonly IProductImportFileReader _reader;
    private readonly ProductImportService _importService;
    private readonly CategoryService _categoryService;
    private readonly CollectionService _collectionService;
    private readonly ProductImportNormalizer _normalizer = new();

    public ProductImportCliService(
        IProductImportFileReader reader,
        ProductImportService importService,
        CategoryService categoryService,
        CollectionService collectionService)
    {
        _reader = reader;
        _importService = importService;
        _categoryService = categoryService;
        _collectionService = collectionService;
    }

    public async Task<int> RunAsync(string filePath, bool apply)
    {
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"Excel dosyası bulunamadı: {filePath}");
            return 2;
        }

        ExcelReadResult source;
        await using (var stream = File.OpenRead(filePath))
        {
            source = _reader.Read(stream);
        }

        Console.WriteLine($"Kaynak sayfa: ilk sayfa (ÜRÜN GAMI), satır: {source.Rows.Count}");
        var initial = await PreviewAsync(filePath);
        PrintPreview("İlk dry-run", initial);

        var categoryNames = source.Rows.Select(r => _normalizer.NullIfBlankOrDash(r.Category))
            .Where(x => x is not null).Select(x => x!).Distinct(StringComparer.CurrentCultureIgnoreCase).Order().ToList();
        var collectionNames = source.Rows.Select(r => _normalizer.NullIfBlankOrDash(r.Series))
            .Where(x => x is not null).Select(x => x!).Distinct(StringComparer.CurrentCultureIgnoreCase).Order().ToList();

        var existingCategories = await _categoryService.GetTreeAsync();
        var existingCollections = await _collectionService.GetAllAsync();
        var missingCategories = categoryNames.Except(existingCategories.Select(x => x.DisplayName ?? ""), StringComparer.CurrentCultureIgnoreCase).ToList();
        var missingCollections = collectionNames.Except(existingCollections.Select(x => x.DisplayName ?? ""), StringComparer.CurrentCultureIgnoreCase).ToList();
        Console.WriteLine($"Eksik kategori: {missingCategories.Count}; eksik koleksiyon/seri: {missingCollections.Count}");
        if (missingCategories.Count > 0) Console.WriteLine("Kategoriler: " + string.Join(", ", missingCategories));
        if (!apply) return initial.BlockingErrors == 0 ? 0 : 1;

        var languages = await _categoryService.GetActiveLanguagesAsync();
        var tr = languages.First(x => string.Equals(x.Code, "TR", StringComparison.OrdinalIgnoreCase));
        foreach (var name in missingCategories)
        {
            var brands = source.Rows.Where(r => string.Equals(_normalizer.NullIfBlankOrDash(r.Category), name, StringComparison.CurrentCultureIgnoreCase))
                .Select(r => _normalizer.NormalizeBrand(r.Brand)).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var result = await _categoryService.CreateAsync(new CreateCategoryRequest
            {
                DisplayOrder = await _categoryService.GetNextDisplayOrderAsync(null),
                Brands = brands.Count > 0 ? brands : [ProductBrand.NgSeramik],
                Translations = [new CategoryTranslationInput { LanguageId = tr.Id, Name = name }]
            });
            if (!result.Succeeded) throw new InvalidOperationException($"Kategori oluşturulamadı ({name}): {result.ErrorMessage}");
        }

        foreach (var name in missingCollections)
        {
            var brands = source.Rows.Where(r => string.Equals(_normalizer.NullIfBlankOrDash(r.Series), name, StringComparison.CurrentCultureIgnoreCase))
                .Select(r => _normalizer.NormalizeBrand(r.Brand)).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var result = await _collectionService.CreateAsync(new CreateCollectionRequest
            {
                DisplayOrder = await _collectionService.GetNextDisplayOrderAsync(),
                Brands = brands.Count > 0 ? brands : [ProductBrand.NgSeramik],
                Translations = [new CollectionTranslationInput { LanguageId = tr.Id, Name = name }]
            });
            if (!result.Succeeded) throw new InvalidOperationException($"Koleksiyon oluşturulamadı ({name}): {result.ErrorMessage}");
        }

        var ready = await PreviewAsync(filePath);
        PrintPreview("Kategori/koleksiyon hazırlığı sonrası dry-run", ready);
        if (!ready.HeaderValid)
        {
            Console.Error.WriteLine("Geçersiz başlıklar nedeniyle import başlatılmadı.");
            PrintTopErrors(ready);
            return 1;
        }
        if (ready.BlockingErrors > 0)
        {
            Console.WriteLine($"{ready.BlockingErrors} hatalı satır atlanacak; {ready.ImportableRows} geçerli satır transaction içinde yüklenecek.");
            PrintTopErrors(ready);
        }

        ProductImportResultDto imported;
        await using (var stream = File.OpenRead(filePath))
        {
            imported = await _importService.ImportAsync(stream);
        }
        Console.WriteLine($"IMPORT SONUCU total={imported.Total} succeeded={imported.Succeeded} created={imported.Created} updated={imported.Updated} skipped={imported.Skipped} failed={imported.Failed} rollback={imported.TransactionRolledBack}");
        return imported.Failed == 0 && !imported.TransactionRolledBack ? 0 : 1;
    }

    private async Task<ProductImportParseResultDto> PreviewAsync(string filePath)
    {
        await using var stream = File.OpenRead(filePath);
        return await _importService.PreviewAsync(stream);
    }

    private static void PrintPreview(string title, ProductImportParseResultDto preview)
    {
        Console.WriteLine($"{title}: header={preview.HeaderValid} total={preview.TotalRows} valid={preview.ImportableRows} errors={preview.BlockingErrors} warningRows={preview.WarningRows} new={preview.NewProducts} update={preview.UpdatedProducts}");
        if (preview.HeaderErrors.Count > 0) Console.WriteLine("Başlık hataları: " + string.Join(" | ", preview.HeaderErrors));
        PrintTopErrors(preview);
    }

    private static void PrintTopErrors(ProductImportParseResultDto preview)
    {
        foreach (var row in preview.Rows.Where(x => !x.IsValid).Take(20))
            Console.WriteLine($"Satır {row.RowNumber} [{row.ProductCode}]: {string.Join(" | ", row.Errors)}");
    }
}
