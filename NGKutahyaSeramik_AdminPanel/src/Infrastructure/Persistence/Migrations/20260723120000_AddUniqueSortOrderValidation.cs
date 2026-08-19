using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueSortOrderValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            NormalizeScopedDisplayOrder(migrationBuilder, "Categories", "ParentCategoryId");
            NormalizeGlobalDisplayOrder(migrationBuilder, "Collections");
            NormalizeGlobalDisplayOrder(migrationBuilder, "Products");
            NormalizeGlobalDisplayOrder(migrationBuilder, "Banners");
            NormalizeGlobalDisplayOrder(migrationBuilder, "BlogCategories");
            NormalizeGlobalDisplayOrder(migrationBuilder, "NewsCategories");
            NormalizeDocumentDisplayOrder(migrationBuilder);
            NormalizeGlobalDisplayOrder(migrationBuilder, "ReferenceProjects");
            NormalizeGlobalDisplayOrder(migrationBuilder, "Languages");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "ReferenceProjects",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "NewsCategories",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "Languages",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "Collections",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "BlogCategories",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "Banners",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "UX_Categories_ParentCategoryId_DisplayOrder",
                table: "Categories",
                columns: new[] { "ParentCategoryId", "DisplayOrder" },
                unique: true,
                filter: "[ParentCategoryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Categories_Root_DisplayOrder",
                table: "Categories",
                column: "DisplayOrder",
                unique: true,
                filter: "[ParentCategoryId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Collections_DisplayOrder",
                table: "Collections",
                column: "DisplayOrder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Products_DisplayOrder",
                table: "Products",
                column: "DisplayOrder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Banners_DisplayOrder",
                table: "Banners",
                column: "DisplayOrder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_BlogCategories_DisplayOrder",
                table: "BlogCategories",
                column: "DisplayOrder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_NewsCategories_DisplayOrder",
                table: "NewsCategories",
                column: "DisplayOrder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Documents_DocumentType_LanguageId_DisplayOrder",
                table: "Documents",
                columns: new[] { "DocumentType", "LanguageId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ReferenceProjects_DisplayOrder",
                table: "ReferenceProjects",
                column: "DisplayOrder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Languages_DisplayOrder",
                table: "Languages",
                column: "DisplayOrder",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "UX_Categories_ParentCategoryId_DisplayOrder", table: "Categories");
            migrationBuilder.DropIndex(name: "UX_Categories_Root_DisplayOrder", table: "Categories");
            migrationBuilder.DropIndex(name: "UX_Collections_DisplayOrder", table: "Collections");
            migrationBuilder.DropIndex(name: "UX_Products_DisplayOrder", table: "Products");
            migrationBuilder.DropIndex(name: "UX_Banners_DisplayOrder", table: "Banners");
            migrationBuilder.DropIndex(name: "UX_BlogCategories_DisplayOrder", table: "BlogCategories");
            migrationBuilder.DropIndex(name: "UX_NewsCategories_DisplayOrder", table: "NewsCategories");
            migrationBuilder.DropIndex(name: "UX_Documents_DocumentType_LanguageId_DisplayOrder", table: "Documents");
            migrationBuilder.DropIndex(name: "UX_ReferenceProjects_DisplayOrder", table: "ReferenceProjects");
            migrationBuilder.DropIndex(name: "UX_Languages_DisplayOrder", table: "Languages");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.AlterColumn<int>(name: "DisplayOrder", table: "ReferenceProjects", type: "int", nullable: false, defaultValue: 0, oldClrType: typeof(int), oldType: "int", oldDefaultValue: 1);
            migrationBuilder.AlterColumn<int>(name: "DisplayOrder", table: "Products", type: "int", nullable: false, defaultValue: 0, oldClrType: typeof(int), oldType: "int", oldDefaultValue: 1);
            migrationBuilder.AlterColumn<int>(name: "DisplayOrder", table: "NewsCategories", type: "int", nullable: false, defaultValue: 0, oldClrType: typeof(int), oldType: "int", oldDefaultValue: 1);
            migrationBuilder.AlterColumn<int>(name: "DisplayOrder", table: "Languages", type: "int", nullable: false, defaultValue: 0, oldClrType: typeof(int), oldType: "int", oldDefaultValue: 1);
            migrationBuilder.AlterColumn<int>(name: "DisplayOrder", table: "Documents", type: "int", nullable: false, defaultValue: 0, oldClrType: typeof(int), oldType: "int", oldDefaultValue: 1);
            migrationBuilder.AlterColumn<int>(name: "DisplayOrder", table: "Collections", type: "int", nullable: false, defaultValue: 0, oldClrType: typeof(int), oldType: "int", oldDefaultValue: 1);
            migrationBuilder.AlterColumn<int>(name: "DisplayOrder", table: "Categories", type: "int", nullable: false, defaultValue: 0, oldClrType: typeof(int), oldType: "int", oldDefaultValue: 1);
            migrationBuilder.AlterColumn<int>(name: "DisplayOrder", table: "BlogCategories", type: "int", nullable: false, defaultValue: 0, oldClrType: typeof(int), oldType: "int", oldDefaultValue: 1);
            migrationBuilder.AlterColumn<int>(name: "DisplayOrder", table: "Banners", type: "int", nullable: false, defaultValue: 0, oldClrType: typeof(int), oldType: "int", oldDefaultValue: 1);
        }

        private static void NormalizeGlobalDisplayOrder(MigrationBuilder migrationBuilder, string table) =>
            migrationBuilder.Sql($"""
                WITH Ranked AS (
                    SELECT Id,
                           ROW_NUMBER() OVER (ORDER BY CASE WHEN DisplayOrder <= 0 THEN 1 ELSE 0 END, DisplayOrder, Id) AS NewDisplayOrder
                    FROM [{table}]
                )
                UPDATE Target
                SET DisplayOrder = Ranked.NewDisplayOrder
                FROM [{table}] AS Target
                INNER JOIN Ranked ON Ranked.Id = Target.Id;
                """);

        private static void NormalizeScopedDisplayOrder(MigrationBuilder migrationBuilder, string table, string scopeColumn) =>
            migrationBuilder.Sql($"""
                WITH Ranked AS (
                    SELECT Id,
                           ROW_NUMBER() OVER (PARTITION BY [{scopeColumn}] ORDER BY CASE WHEN DisplayOrder <= 0 THEN 1 ELSE 0 END, DisplayOrder, Id) AS NewDisplayOrder
                    FROM [{table}]
                )
                UPDATE Target
                SET DisplayOrder = Ranked.NewDisplayOrder
                FROM [{table}] AS Target
                INNER JOIN Ranked ON Ranked.Id = Target.Id;
                """);

        private static void NormalizeDocumentDisplayOrder(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql("""
                WITH Ranked AS (
                    SELECT Id,
                           ROW_NUMBER() OVER (PARTITION BY DocumentType, LanguageId ORDER BY CASE WHEN DisplayOrder <= 0 THEN 1 ELSE 0 END, DisplayOrder, Id) AS NewDisplayOrder
                    FROM [Documents]
                )
                UPDATE Target
                SET DisplayOrder = Ranked.NewDisplayOrder
                FROM [Documents] AS Target
                INNER JOIN Ranked ON Ranked.Id = Target.Id;
                """);
    }
}
