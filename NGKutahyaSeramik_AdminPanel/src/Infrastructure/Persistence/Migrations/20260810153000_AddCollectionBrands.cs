using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260810153000_AddCollectionBrands")]
public partial class AddCollectionBrands : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "BrandCodes",
            table: "Collections",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "NgSeramik,NgStone,NgSlim,NgPerforma");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "BrandCodes", table: "Collections");
    }
}
