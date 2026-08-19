using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReferenceProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Location = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ProjectType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Architect = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductReferenceProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ReferenceProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReferenceProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductReferenceProjects_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductReferenceProjects_ReferenceProjects_ReferenceProjectId",
                        column: x => x.ReferenceProjectId,
                        principalTable: "ReferenceProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReferenceProjectImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferenceProjectId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceProjectImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferenceProjectImages_ReferenceProjects_ReferenceProjectId",
                        column: x => x.ReferenceProjectId,
                        principalTable: "ReferenceProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductReferenceProjects_ProductId_ReferenceProjectId",
                table: "ProductReferenceProjects",
                columns: new[] { "ProductId", "ReferenceProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductReferenceProjects_ReferenceProjectId",
                table: "ProductReferenceProjects",
                column: "ReferenceProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceProjectImages_ReferenceProjectId_IsFeatured",
                table: "ReferenceProjectImages",
                column: "ReferenceProjectId",
                unique: true,
                filter: "[IsFeatured] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductReferenceProjects");

            migrationBuilder.DropTable(
                name: "ReferenceProjectImages");

            migrationBuilder.DropTable(
                name: "ReferenceProjects");
        }
    }
}
