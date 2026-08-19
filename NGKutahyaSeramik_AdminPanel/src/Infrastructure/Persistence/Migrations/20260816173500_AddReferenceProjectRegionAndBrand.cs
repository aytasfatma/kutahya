using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260816173500_AddReferenceProjectRegionAndBrand")]
public partial class AddReferenceProjectRegionAndBrand : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Region",
            table: "ReferenceProjects",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "Domestic");

        migrationBuilder.AddColumn<string>(
            name: "Brand",
            table: "ReferenceProjects",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "NgSeramik");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Region", table: "ReferenceProjects");
        migrationBuilder.DropColumn(name: "Brand", table: "ReferenceProjects");
    }
}
