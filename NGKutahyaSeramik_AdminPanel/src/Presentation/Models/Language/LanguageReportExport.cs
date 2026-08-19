using System.Globalization;
using System.Text;
using Application.Translations;
using ClosedXML.Excel;

namespace Presentation.Models.Language;

public static class LanguageReportExport
{
    public static byte[] ToExcel(IReadOnlyList<MissingTranslationDto> items, Func<MissingTranslationDto, string> editUrl)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Eksik Çeviriler");

        var headers = new[]
        {
            "Modül", "Kayıt", "Kayıt ID", "Dil", "Dil Kodu", "Alan", "Alan Görünen Adı", "Öncelik", "Düzenleme URL'si"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
        }

        for (var i = 0; i < items.Count; i++)
        {
            var row = items[i];
            var index = i + 2;
            sheet.Cell(index, 1).Value = row.ModuleLabel;
            sheet.Cell(index, 2).Value = row.DisplayName;
            sheet.Cell(index, 3).Value = row.EntityId;
            sheet.Cell(index, 4).Value = row.LanguageName;
            sheet.Cell(index, 5).Value = row.LanguageCode;
            sheet.Cell(index, 6).Value = row.FieldName;
            sheet.Cell(index, 7).Value = LanguageReportLabels.Field(row.FieldName);
            sheet.Cell(index, 8).Value = LanguageReportLabels.Priority(row.FieldName);
            sheet.Cell(index, 9).Value = editUrl(row);
        }

        sheet.RangeUsed()?.SetAutoFilter();
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public static byte[] ToPdf(
        IReadOnlyList<MissingTranslationDto> items,
        LanguageReportQueryViewModel query,
        Func<MissingTranslationDto, string> editUrl)
    {
        var lines = new List<string>
        {
            "Eksik Çeviri Raporu",
            $"Oluşturulma tarihi: {DateTime.Now.ToString("dd.MM.yyyy HH:mm", CultureInfo.GetCultureInfo("tr-TR"))}",
            $"Aktif filtreler: {DescribeFilters(query)}",
            $"Toplam sonuç: {items.Count}",
            "",
            "Modül | Kayıt | Dil | Alan | Öncelik | Düzenleme URL'si"
        };

        lines.AddRange(items.Take(120).Select(i =>
            $"{i.ModuleLabel} | {i.DisplayName} | {i.LanguageCode} | {LanguageReportLabels.Field(i.FieldName)} | {LanguageReportLabels.Priority(i.FieldName)} | {editUrl(i)}"));

        if (items.Count > 120)
        {
            lines.Add($"... {items.Count - 120} ek satır Excel çıktısında yer alır.");
        }

        return SimplePdfWriter.Write(lines);
    }

    private static string DescribeFilters(LanguageReportQueryViewModel query)
    {
        var parts = new List<string>();
        if (query.Type is not null) parts.Add($"Modül={query.Type}");
        if (query.EntityId is not null) parts.Add($"Kayıt={query.EntityId}");
        if (query.LanguageId is not null) parts.Add($"Dil={query.LanguageId}");
        if (!string.IsNullOrWhiteSpace(query.Field)) parts.Add($"Alan={query.Field}");
        if (!string.IsNullOrWhiteSpace(query.Search)) parts.Add($"Arama={query.Search}");
        return parts.Count == 0 ? "Yok" : string.Join(", ", parts);
    }
}

public static class LanguageReportLabels
{
    private static readonly Dictionary<string, string> FieldLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Name"] = "Ad",
        ["Title"] = "Başlık",
        ["ProjectName"] = "Proje Adı",
        ["Description"] = "Açıklama",
        ["ShortDescription"] = "Kısa Açıklama",
        ["LongDescription"] = "Uzun Açıklama",
        ["Content"] = "İçerik",
        ["Excerpt"] = "Özet",
        ["SeoUrl"] = "SEO URL",
        ["MetaTitle"] = "Meta Başlık",
        ["MetaDescription"] = "Meta Açıklama",
        ["Subtitle"] = "Alt Başlık",
        ["ButtonText"] = "Buton Metni",
        ["ButtonUrl"] = "Buton URL"
    };

    public static string Field(string fieldName) =>
        FieldLabels.GetValueOrDefault(fieldName, fieldName);

    public static string Priority(string fieldName) =>
        fieldName.Contains("Meta", StringComparison.OrdinalIgnoreCase) ||
        fieldName.Contains("Seo", StringComparison.OrdinalIgnoreCase)
            ? "Orta"
            : "Yüksek";
}

internal static class SimplePdfWriter
{
    public static byte[] Write(IReadOnlyList<string> lines)
    {
        var content = new StringBuilder();
        content.AppendLine("BT");
        content.AppendLine("/F1 10 Tf");
        content.AppendLine("50 790 Td");

        foreach (var line in lines)
        {
            foreach (var chunk in Split(line, 105))
            {
                content.Append('<').Append(ToUtf16Hex(chunk)).AppendLine("> Tj");
                content.AppendLine("0 -14 Td");
            }
        }

        content.AppendLine("ET");
        var contentBytes = Encoding.ASCII.GetBytes(content.ToString());
        var objects = new List<string>
        {
            "1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj\n",
            "2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj\n",
            "3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >> endobj\n",
            "4 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj\n",
            $"5 0 obj << /Length {contentBytes.Length} >> stream\n{content}endstream\nendobj\n"
        };

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("%PDF-1.4\n");
        var offsets = new List<long> { 0 };

        foreach (var obj in objects)
        {
            writer.Flush();
            offsets.Add(stream.Position);
            writer.Write(obj);
        }

        writer.Flush();
        var xrefOffset = stream.Position;
        writer.Write($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
        {
            writer.Write($"{offset:0000000000} 00000 n \n");
        }

        writer.Write($"trailer << /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
        writer.Flush();
        return stream.ToArray();
    }

    private static IEnumerable<string> Split(string value, int length)
    {
        if (string.IsNullOrEmpty(value))
        {
            yield return string.Empty;
            yield break;
        }

        for (var i = 0; i < value.Length; i += length)
        {
            yield return value.Substring(i, Math.Min(length, value.Length - i));
        }
    }

    private static string ToUtf16Hex(string value)
    {
        var bytes = Encoding.BigEndianUnicode.GetBytes(value);
        return "FEFF" + Convert.ToHexString(bytes);
    }
}
