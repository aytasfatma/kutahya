using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Infrastructure.Persistence.Migrations;

[Migration("20260818110000_AddDocumentBrand")]
[DbContext(typeof(AppDbContext))]
public partial class AddDocumentBrand : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "Brand", table: "Documents", type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "NgSeramik");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Brand", table: "Documents");
    }
}
