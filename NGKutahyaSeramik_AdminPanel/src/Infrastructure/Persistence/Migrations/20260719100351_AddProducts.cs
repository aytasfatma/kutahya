using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CollectionId = table.Column<int>(type: "int", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Size = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Surface = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Relief = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SpecialSurface = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FaceCount = table.Column<int>(type: "int", nullable: true),
                    Thickness = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    BodyType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ColorMaterial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApplicationArea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UsageArea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Finish = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PEI = table.Column<decimal>(type: "decimal(10,3)", nullable: true),
                    VValue = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RValue = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DeepAbrasion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HeatResistance = table.Column<bool>(type: "bit", nullable: true),
                    AntiSlip = table.Column<bool>(type: "bit", nullable: true),
                    GlazedGranite = table.Column<bool>(type: "bit", nullable: true),
                    BoxM2 = table.Column<decimal>(type: "decimal(10,3)", nullable: true),
                    PalletM2 = table.Column<decimal>(type: "decimal(10,3)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CollectionId",
                table: "Products",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCode",
                table: "Products",
                column: "ProductCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
