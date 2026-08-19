using Application.ProductImport;
using Domain.Enums;
using FluentAssertions;

namespace NGKutahyaSeramik.UnitTests.Services;

/// <summary>Backlog #17 devamı — Madde 37.3 raporlama. `ProductImportReportBuilder` saf/statik bir
/// dönüştürücü, DB/dosya gerekmez — `ProductImportResultDto`'yu doğrudan elle kurup besliyoruz.</summary>
public class ProductImportReportBuilderTests
{
    private static ProductImportRowResult ValidNewRow(int rowNumber = 2, string productCode = "IMP0001", ProductStatus status = ProductStatus.Active) => new()
    {
        RowNumber = rowNumber,
        ProductCode = productCode,
        ProductName = "Test Ürün",
        IsValid = true,
        Action = ProductImportRowAction.New,
        Errors = [],
        ResolvedStatus = status,
        ResolvedCategoryId = 1,
        ResolvedCollectionId = 1,
        ResolvedSize = "60x60",
        ResolvedSurface = "Mat",
        ResolvedColor = "Beyaz"
    };

    [Fact]
    public void Build_ValidNewRow_ProducesCreatedOutcome()
    {
        var result = new ProductImportResultDto
        {
            Total = 1,
            Succeeded = 1,
            Created = 1,
            RowResults = [ValidNewRow()]
        };

        var report = ProductImportReportBuilder.Build("test.xlsx", result);

        report.Rows.Should().ContainSingle();
        report.Rows[0].Outcome.Should().Be(ProductImportReportOutcome.Created);
        report.Rows[0].ErrorMessage.Should().BeNull();
        report.Rows[0].HasMissingRequiredField.Should().BeFalse();
        report.CreatedRows.Should().HaveCount(1);
    }

    [Fact]
    public void Build_ValidUpdateRow_ProducesUpdatedOutcome()
    {
        var updateRow = new ProductImportRowResult
        {
            RowNumber = 3,
            ProductCode = "IMP0002",
            ProductName = "Var Olan Ürün",
            IsValid = true,
            Action = ProductImportRowAction.Update,
            ExistingProductId = 5,
            ResolvedStatus = ProductStatus.Active,
            ResolvedSize = "60x60",
            ResolvedSurface = "Mat",
            ResolvedColor = "Beyaz"
        };

        var result = new ProductImportResultDto { Total = 1, Succeeded = 1, Updated = 1, RowResults = [updateRow] };
        var report = ProductImportReportBuilder.Build("test.xlsx", result);

        report.Rows[0].Outcome.Should().Be(ProductImportReportOutcome.Updated);
        report.UpdatedRows.Should().HaveCount(1);
    }

    [Fact]
    public void Build_InvalidRow_WithMissingRequiredFieldError_FlagsHasMissingRequiredField()
    {
        var invalidRow = new ProductImportRowResult
        {
            RowNumber = 4,
            ProductCode = "IMP0003",
            ProductName = null,
            IsValid = false,
            Action = ProductImportRowAction.Skipped,
            Errors = ["ProductName zorunludur."]
        };

        var result = new ProductImportResultDto { Total = 1, Failed = 1, RowResults = [invalidRow] };
        var report = ProductImportReportBuilder.Build("test.xlsx", result);

        report.Rows[0].Outcome.Should().Be(ProductImportReportOutcome.Failed);
        report.Rows[0].HasMissingRequiredField.Should().BeTrue();
        report.Rows[0].ErrorMessage.Should().Contain("ProductName zorunludur.");
        report.MissingRequiredFieldRows.Should().HaveCount(1);
        report.FailedRows.Should().HaveCount(1);
    }

    [Fact]
    public void Build_InvalidRow_WithNonRequiredFieldError_DoesNotFlagMissingRequiredField()
    {
        var invalidRow = new ProductImportRowResult
        {
            RowNumber = 5,
            ProductCode = "IMP0004",
            IsValid = false,
            Action = ProductImportRowAction.Skipped,
            Errors = ["Geçersiz FaceCount değeri: 'abc'. Pozitif tam sayı olmalı."]
        };

        var result = new ProductImportResultDto { Total = 1, Failed = 1, RowResults = [invalidRow] };
        var report = ProductImportReportBuilder.Build("test.xlsx", result);

        report.Rows[0].HasMissingRequiredField.Should().BeFalse();
        report.MissingRequiredFieldRows.Should().BeEmpty();
    }

    [Fact]
    public void Build_ValidRowWithInactiveStatus_AndCreatedOutcome_FlagsIsInactiveProduct()
    {
        var row = ValidNewRow(status: ProductStatus.Inactive);
        var result = new ProductImportResultDto { Total = 1, Succeeded = 1, Created = 1, RowResults = [row] };

        var report = ProductImportReportBuilder.Build("test.xlsx", result);

        report.Rows[0].IsInactiveProduct.Should().BeTrue();
        report.InactiveProductRows.Should().HaveCount(1);
    }

    [Fact]
    public void Build_ValidRowWithActiveStatus_DoesNotFlagInactiveProduct()
    {
        var row = ValidNewRow(status: ProductStatus.Active);
        var result = new ProductImportResultDto { Total = 1, Succeeded = 1, Created = 1, RowResults = [row] };

        var report = ProductImportReportBuilder.Build("test.xlsx", result);

        report.Rows[0].IsInactiveProduct.Should().BeFalse();
        report.InactiveProductRows.Should().BeEmpty();
    }

    // Kritik senaryo: transaction rollback olduğunda, parse-zamanında IsValid=true olan satırlar bile
    // GERÇEKTE yazılmadı — rapor bunu "Yeni/Güncellendi" değil "Hatalı" olarak yansıtmalı.
    [Fact]
    public void Build_TransactionRolledBack_MarksEvenValidRowsAsFailed()
    {
        var validRow = ValidNewRow();
        var result = new ProductImportResultDto
        {
            Total = 1,
            Failed = 1,
            RowResults = [validRow],
            TransactionRolledBack = true
        };

        var report = ProductImportReportBuilder.Build("test.xlsx", result);

        report.Rows[0].Outcome.Should().Be(ProductImportReportOutcome.Failed);
        report.Rows[0].ErrorMessage.Should().Contain("geri alındı");
        report.CreatedRows.Should().BeEmpty();
        report.FailedRows.Should().HaveCount(1);
    }

    [Fact]
    public void Build_MixedRows_CountsAndCategoriesAreCorrect()
    {
        var created = ValidNewRow(rowNumber: 2, productCode: "A");
        var updated = new ProductImportRowResult { RowNumber = 3, ProductCode = "B", IsValid = true, Action = ProductImportRowAction.Update, ExistingProductId = 1, ResolvedStatus = ProductStatus.Active };
        var failed = new ProductImportRowResult { RowNumber = 4, ProductCode = "C", IsValid = false, Errors = ["Size zorunludur."] };
        var inactive = ValidNewRow(rowNumber: 5, productCode: "D", status: ProductStatus.Inactive);

        var result = new ProductImportResultDto
        {
            Total = 4,
            Succeeded = 3,
            Created = 2,
            Updated = 1,
            Failed = 1,
            RowResults = [created, updated, failed, inactive]
        };

        var report = ProductImportReportBuilder.Build("mixed.xlsx", result);

        report.Total.Should().Be(4);
        report.Created.Should().Be(2);
        report.Updated.Should().Be(1);
        report.Failed.Should().Be(1);
        report.MissingRequiredFieldRows.Should().HaveCount(1);
        report.InactiveProductRows.Should().HaveCount(1);
        report.FileName.Should().Be("mixed.xlsx");
    }
}
