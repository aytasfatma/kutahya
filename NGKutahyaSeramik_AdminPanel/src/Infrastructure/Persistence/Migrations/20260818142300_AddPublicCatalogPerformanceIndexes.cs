using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations;

[Migration("20260818142300_AddPublicCatalogPerformanceIndexes")]
[DbContext(typeof(AppDbContext))]
public partial class AddPublicCatalogPerformanceIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Products_Status_DisplayOrder",
            table: "Products",
            columns: new[] { "Status", "DisplayOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_Products_Status_Collection_DisplayOrder",
            table: "Products",
            columns: new[] { "Status", "CollectionId", "DisplayOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_Products_Status_Category_DisplayOrder",
            table: "Products",
            columns: new[] { "Status", "CategoryId", "DisplayOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_Products_Status_Brand_DisplayOrder",
            table: "Products",
            columns: new[] { "Status", "Brand", "DisplayOrder" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Products_Status_DisplayOrder", table: "Products");
        migrationBuilder.DropIndex(name: "IX_Products_Status_Collection_DisplayOrder", table: "Products");
        migrationBuilder.DropIndex(name: "IX_Products_Status_Category_DisplayOrder", table: "Products");
        migrationBuilder.DropIndex(name: "IX_Products_Status_Brand_DisplayOrder", table: "Products");
    }
}
