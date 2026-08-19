using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceAndConstraintIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PageContentBlocks_PageId",
                table: "PageContentBlocks");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_DisplayOrder",
                table: "ProductImages",
                columns: new[] { "ProductId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PageContentBlocks_PageId_DisplayOrder",
                table: "PageContentBlocks",
                columns: new[] { "PageId", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductId_DisplayOrder",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_PageContentBlocks_PageId_DisplayOrder",
                table: "PageContentBlocks");

            migrationBuilder.CreateIndex(
                name: "IX_PageContentBlocks_PageId",
                table: "PageContentBlocks",
                column: "PageId");
        }
    }
}
