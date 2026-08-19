using Application.ProductImport;
using ClosedXML.Excel;

namespace Infrastructure.ProductImport;

public class ExcelProductImportTemplateWriter : IProductImportTemplateWriter
{
    private static readonly (string Header, string Note)[] Columns =
    [
        ("Ürün Kodu", "Zorunlu, tekil. Mevcut ürün kodu varsa güncellenir."),
        ("Seri Adı", "Zorunlu. Sistemdeki koleksiyon adıyla eşleşmelidir."),
        ("Ürün Segmentasyonu", "Örnek: NG SERAMİK, NG STONE, NG SLIM."),
        ("Ürün Adı", "Zorunlu. Türkçe ürün adı."),
        ("Koleksiyon İsimleri", "Ticari/uzun ürün adı."),
        ("Durum", "Örnek: DEVAM, AKTİF, PASİF, İPTAL."),
        ("Ürün Grubu", "Opsiyonel ürün grubu."),
        ("Ebat", "Örnek: 60*120 veya 60x120."),
        ("Birim", "M2 veya ADT."),
        ("Yüzey", "Örnek: MAT, NANO, PARLAK."),
        ("Rölyef", "Örnek: DÜZ, RÖLYEFLİ."),
        ("Özel Yüzey", "Opsiyonel."),
        ("Face Görseli", "var/yok veya EVET/HAYIR."),
        ("Mekân Görseli", "var/yok veya EVET/HAYIR."),
        ("Face Sayısı", "Pozitif tam sayı."),
        ("Kategori", "Zorunlu. Sistemdeki kategori adıyla eşleşmelidir."),
        ("Kalınlık (mm)", "Örnek: 8,0 mm veya 8.0."),
        ("Bünye", "Sırlı Porselen, Duvar Karosu, Teknik Granit Seramik."),
        ("V Değeri", "V1, V2, V3 veya V4."),
        ("PEI Değeri", "1 ile 5 arasında."),
        ("R Değeri", "R9, R10, R11, R12, R13 veya R11-R12."),
        ("Derin Aşınma", "Opsiyonel metin."),
        ("Renk", "Ürün tanımındaki renk."),
        ("Malzeme Rengi", "Renk malzeme grubu."),
        ("Bitiş", "Rektifiyeli veya Rektifiyesiz."),
        ("Isıya Dayanıklılık", "EVET/HAYIR."),
        ("Kaymazlık", "HAYIR, EVET, R11'e uygun, R11-R12 aralığına uygun."),
        ("Uygulama Alanı", "YER, DUVAR veya YER DUVAR."),
        ("Kullanım Alanı", "BANYO, MUTFAK veya BANYO MUTFAK."),
        ("Sırlı Granit", "EVET/HAYIR."),
        ("Kutu m²", "Negatif olmayan sayı."),
        ("Palet m²", "Negatif olmayan sayı.")
    ];

    public byte[] GenerateTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Ürünler");

        for (var i = 0; i < Columns.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = Columns[i].Header;
            cell.Style.Font.Bold = true;
            worksheet.Cell(2, i + 1).Value = Columns[i].Note;
        }

        worksheet.Row(2).Style.Font.Italic = true;
        worksheet.Row(2).Style.Font.FontColor = XLColor.Gray;
        worksheet.SheetView.FreezeRows(1);
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
