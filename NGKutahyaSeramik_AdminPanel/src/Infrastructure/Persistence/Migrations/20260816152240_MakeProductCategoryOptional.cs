using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeProductCategoryOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql("""
                DECLARE @UncategorizedIds TABLE ([Id] int PRIMARY KEY);

                INSERT INTO @UncategorizedIds ([Id])
                SELECT DISTINCT c.[Id]
                FROM [Categories] c
                INNER JOIN [Translations] t
                    ON t.[EntityType] = 'Category'
                   AND t.[EntityId] = c.[Id]
                   AND t.[FieldName] = 'Name'
                WHERE UPPER(LTRIM(RTRIM(t.[Value]))) IN
                (
                    UPPER(N'Kategorisiz'),
                    UPPER(N'Genel (-kategorisiz-)'),
                    UPPER(N'-Kategorisiz-')
                );

                UPDATE [Products]
                SET [CategoryId] = NULL
                WHERE [CategoryId] IN (SELECT [Id] FROM @UncategorizedIds);

                DELETE FROM [Translations]
                WHERE [EntityType] = 'Category'
                  AND [EntityId] IN (SELECT [Id] FROM @UncategorizedIds);

                DELETE FROM [Categories]
                WHERE [Id] IN (SELECT [Id] FROM @UncategorizedIds);
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [Products] WHERE [CategoryId] IS NULL)
                BEGIN
                    DECLARE @DisplayOrder int = ISNULL((SELECT MAX([DisplayOrder]) FROM [Categories]), 0) + 1;
                    INSERT INTO [Categories] ([ParentCategoryId], [ImagePath], [BrandCodes], [DisplayOrder], [IsActive])
                    VALUES (NULL, NULL, N'NgSeramik,NgStone,NgSlim,NgPerforma', @DisplayOrder, 1);

                    DECLARE @CategoryId int = CONVERT(int, SCOPE_IDENTITY());
                    DECLARE @LanguageId int = (SELECT TOP (1) [Id] FROM [Languages] WHERE UPPER([Code]) = 'TR');

                    IF @LanguageId IS NOT NULL
                        INSERT INTO [Translations] ([EntityType], [EntityId], [LanguageId], [FieldName], [Value])
                        VALUES ('Category', @CategoryId, @LanguageId, 'Name', N'Kategorisiz');

                    UPDATE [Products] SET [CategoryId] = @CategoryId WHERE [CategoryId] IS NULL;
                END
                """);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
