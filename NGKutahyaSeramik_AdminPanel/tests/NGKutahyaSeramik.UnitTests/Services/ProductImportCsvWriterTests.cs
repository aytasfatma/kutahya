using Application.ProductImport;
using FluentAssertions;

namespace NGKutahyaSeramik.UnitTests.Services;

public class ProductImportCsvWriterTests
{
    private static ProductImportReportDto SingleRowReport(string productCode, string? productName = null, string? errorMessage = null) => new()
    {
        FileName = "test.xlsx",
        Rows =
        [
            new ProductImportReportRow
            {
                RowNumber = 2,
                ProductCode = productCode,
                ProductName = productName,
                Outcome = ProductImportReportOutcome.Failed,
                ErrorMessage = errorMessage
            }
        ]
    };

    private static string GetDataLine(string csv) => csv.Replace("\r\n", "\n").Split('\n')[1];

    [Fact]
    public void ToCsv_IncludesHeaderRow()
    {
        var csv = ProductImportCsvWriter.ToCsv(SingleRowReport("IMP0001"));

        csv.Should().StartWith("Excel Satırı,Ürün Kodu,Ürün Adı,Seri,Kategori,Sonuç,Hata,Uyarı,Eksik Zorunlu Alan,Pasif Ürün");
    }

    [Theory]
    [InlineData("=SUM(A1:A10)")]
    [InlineData("+1234567890")]
    [InlineData("-cmd|'/c calc'!A1")]
    [InlineData("@SUM(1+1)")]
    public void ToCsv_ProductCodeStartingWithFormulaTriggerChar_IsPrefixedWithSingleQuote(string maliciousCode)
    {
        var csv = ProductImportCsvWriter.ToCsv(SingleRowReport(maliciousCode));
        var dataLine = GetDataLine(csv);

        dataLine.Should().Contain($"'{maliciousCode}");
        dataLine.Should().NotContain($",{maliciousCode}");
    }

    [Fact]
    public void ToCsv_NormalProductCode_IsNotModified()
    {
        var csv = ProductImportCsvWriter.ToCsv(SingleRowReport("IMP0001"));
        var dataLine = GetDataLine(csv);

        dataLine.Should().StartWith("2,IMP0001,");
    }

    [Fact]
    public void ToCsv_ValueContainingComma_IsQuoted()
    {
        var csv = ProductImportCsvWriter.ToCsv(SingleRowReport("IMP0001", errorMessage: "Hata, ikinci kısım"));

        csv.Should().Contain("\"Hata, ikinci kısım\"");
    }

    [Fact]
    public void ToCsv_ValueContainingDoubleQuote_IsEscapedByDoubling()
    {
        var csv = ProductImportCsvWriter.ToCsv(SingleRowReport("IMP0001", productName: "12\" Ürün"));

        csv.Should().Contain("\"12\"\" Ürün\"");
    }

    [Fact]
    public void ToCsv_NullProductName_RendersAsEmptyField()
    {
        var csv = ProductImportCsvWriter.ToCsv(SingleRowReport("IMP0001", productName: null));
        var dataLine = GetDataLine(csv);

        dataLine.Should().Be("2,IMP0001,,,,Başarısız,,,Hayır,Hayır");
    }

    [Fact]
    public void ToCsv_MissingRequiredFieldAndInactiveFlags_RenderAsYesNo()
    {
        var report = new ProductImportReportDto
        {
            FileName = "test.xlsx",
            Rows =
            [
                new ProductImportReportRow
                {
                    RowNumber = 2,
                    ProductCode = "IMP0001",
                    Outcome = ProductImportReportOutcome.Created,
                    HasMissingRequiredField = false,
                    IsInactiveProduct = true
                }
            ]
        };

        var csv = ProductImportCsvWriter.ToCsv(report);

        csv.Should().Contain("Hayır,Evet");
    }
}
