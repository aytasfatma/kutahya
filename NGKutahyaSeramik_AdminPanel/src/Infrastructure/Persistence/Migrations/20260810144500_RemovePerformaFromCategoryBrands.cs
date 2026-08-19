using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260810144500_RemovePerformaFromCategoryBrands")]
public partial class RemovePerformaFromCategoryBrands : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE [Categories] SET [BrandCodes] = REPLACE(REPLACE([BrandCodes], ',NgPerforma', ''), 'NgPerforma,', '')");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE [Categories] SET [BrandCodes] = CONCAT([BrandCodes], ',NgPerforma') WHERE [BrandCodes] NOT LIKE '%NgPerforma%'");
    }
}
