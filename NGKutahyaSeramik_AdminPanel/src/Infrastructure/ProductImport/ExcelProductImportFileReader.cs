using Application.ProductImport;
using ClosedXML.Excel;

namespace Infrastructure.ProductImport;

/// <summary>
/// Backlog #17 — `IProductImportFileReader`'ın ClosedXML (MIT) tabanlı implementasyonu. Application
/// katmanı ClosedXML'e hiç bağımlı değil, yalnızca bu arayüze (storage abstraction ilkesiyle
/// tutarlı, ADR-006/013). İlk sayfanın ilk satırı header, sonraki dolu satırlar veri kabul edilir.
/// </summary>
public class ExcelProductImportFileReader : IProductImportFileReader
{
    public ExcelReadResult Read(Stream fileStream)
    {
        fileStream.Position = 0;
        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheets.First();

        var headerRow = worksheet.Row(1);
        var lastColumn = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;

        var headers = new List<string>();
        var columnIndexByHeader = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var col = 1; col <= lastColumn; col++)
        {
            var value = headerRow.Cell(col).GetString().Trim();
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            headers.Add(value);
            var canonical = ProductImportHeaders.ResolveCanonical(value);
            columnIndexByHeader[canonical ?? value] = col;
        }

        string? GetCell(IXLRow row, string headerName) =>
            columnIndexByHeader.TryGetValue(headerName, out var col)
                ? NullIfBlank(row.Cell(col).GetString())
                : null;

        IReadOnlyDictionary<string, string?> GetRawValues(IXLRow row) =>
            ProductImportHeaders.CanonicalHeaders.ToDictionary(header => header, header => GetCell(row, header));

        var rows = new List<ProductImportRow>();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var row = worksheet.Row(rowNumber);
            if (row.IsEmpty())
            {
                continue;
            }

            rows.Add(new ProductImportRow
            {
                RowNumber = rowNumber,
                RawValues = GetRawValues(row),
                ProductCode = GetCell(row, "ProductCode"),
                ProductName = GetCell(row, "ProductName"),
                Series = GetCell(row, "Series"),
                Brand = GetCell(row, "Brand"),
                CommercialName = GetCell(row, "CommercialName"),
                Status = GetCell(row, "Status"),
                ProductGroup = GetCell(row, "ProductGroup"),
                Size = GetCell(row, "Size"),
                Unit = GetCell(row, "Unit"),
                Surface = GetCell(row, "Surface"),
                Relief = GetCell(row, "Relief"),
                SpecialSurface = GetCell(row, "SpecialSurface"),
                HasFace = GetCell(row, "HasFace"),
                HasVenue = GetCell(row, "HasVenue"),
                Color = GetCell(row, "Color"),
                Category = GetCell(row, "Category"),
                Thickness = GetCell(row, "Thickness"),
                BodyType = GetCell(row, "BodyType"),
                VValue = GetCell(row, "VValue"),
                PEI = GetCell(row, "PEI"),
                RValue = GetCell(row, "RValue"),
                DeepAbrasion = GetCell(row, "DeepAbrasion"),
                ColorMaterial = GetCell(row, "ColorMaterial"),
                Finish = GetCell(row, "Finish"),
                HeatResistance = GetCell(row, "HeatResistance"),
                AntiSlip = GetCell(row, "AntiSlip"),
                UsageArea = GetCell(row, "UsageArea"),
                ApplicationArea = GetCell(row, "ApplicationArea"),
                GlazedGranite = GetCell(row, "GlazedGranite"),
                FaceCount = GetCell(row, "FaceCount")
                    ,
                BoxM2 = GetCell(row, "BoxM2"),
                PalletM2 = GetCell(row, "PalletM2")
            });
        }

        return new ExcelReadResult(headers, rows);
    }

    private static string? NullIfBlank(string value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
