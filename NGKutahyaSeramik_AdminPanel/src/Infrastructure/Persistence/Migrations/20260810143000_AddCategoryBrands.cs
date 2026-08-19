using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260810143000_AddCategoryBrands")]
public partial class AddCategoryBrands : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "BrandCodes",
            table: "Categories",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "NgSeramik,NgStone,NgSlim");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "BrandCodes", table: "Categories");
    }
}
