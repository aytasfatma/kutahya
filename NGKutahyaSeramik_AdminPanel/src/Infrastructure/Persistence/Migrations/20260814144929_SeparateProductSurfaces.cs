using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeparateProductSurfaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SurfaceId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Surfaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Surfaces", x => x.Id);
                });

            // Mevcut metinsel yüzey verilerini koruyarak yeni ilişki tablosuna taşı.
            migrationBuilder.Sql("""
                ;WITH DistinctSurfaces AS
                (
                    SELECT DISTINCT LTRIM(RTRIM([Surface])) AS [Name]
                    FROM [Products]
                    WHERE [Surface] IS NOT NULL
                      AND LTRIM(RTRIM([Surface])) NOT IN ('', '-')
                ), OrderedSurfaces AS
                (
                    SELECT [Name], ROW_NUMBER() OVER (ORDER BY [Name]) AS [DisplayOrder]
                    FROM DistinctSurfaces
                )
                INSERT INTO [Surfaces] ([Name], [DisplayOrder], [IsActive])
                SELECT [Name], [DisplayOrder], 1 FROM OrderedSurfaces;

                UPDATE p
                SET p.[SurfaceId] = s.[Id]
                FROM [Products] p
                INNER JOIN [Surfaces] s
                    ON s.[Name] = LTRIM(RTRIM(p.[Surface]));
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SurfaceId",
                table: "Products",
                column: "SurfaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Surfaces_DisplayOrder",
                table: "Surfaces",
                column: "DisplayOrder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Surfaces_Name",
                table: "Surfaces",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Surfaces_SurfaceId",
                table: "Products",
                column: "SurfaceId",
                principalTable: "Surfaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Surfaces_SurfaceId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Surfaces");

            migrationBuilder.DropIndex(
                name: "IX_Products_SurfaceId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SurfaceId",
                table: "Products");
        }
    }
}
