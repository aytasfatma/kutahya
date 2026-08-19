using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTaxonomyTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SeoUrl",
                table: "Surfaces",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Collections",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeoUrl",
                table: "Collections",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeoUrl",
                table: "Categories",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE c SET c.[Name]=COALESCE((SELECT TOP (1) t.[Value] FROM [Translations] t INNER JOIN [Languages] l ON l.[Id]=t.[LanguageId] WHERE t.[EntityType]='Category' AND t.[EntityId]=c.[Id] AND t.[FieldName]='Name' ORDER BY CASE WHEN UPPER(l.[Code])='TR' THEN 0 ELSE 1 END),N'Kategori '+CONVERT(nvarchar(20),c.[Id])), c.[SeoUrl]=(SELECT TOP (1) t.[Value] FROM [Translations] t INNER JOIN [Languages] l ON l.[Id]=t.[LanguageId] WHERE t.[EntityType]='Category' AND t.[EntityId]=c.[Id] AND t.[FieldName]='SeoUrl' ORDER BY CASE WHEN UPPER(l.[Code])='TR' THEN 0 ELSE 1 END) FROM [Categories] c;
                UPDATE c SET c.[Name]=COALESCE((SELECT TOP (1) t.[Value] FROM [Translations] t INNER JOIN [Languages] l ON l.[Id]=t.[LanguageId] WHERE t.[EntityType]='Collection' AND t.[EntityId]=c.[Id] AND t.[FieldName]='Name' ORDER BY CASE WHEN UPPER(l.[Code])='TR' THEN 0 ELSE 1 END),N'Koleksiyon '+CONVERT(nvarchar(20),c.[Id])), c.[SeoUrl]=(SELECT TOP (1) t.[Value] FROM [Translations] t INNER JOIN [Languages] l ON l.[Id]=t.[LanguageId] WHERE t.[EntityType]='Collection' AND t.[EntityId]=c.[Id] AND t.[FieldName]='SeoUrl' ORDER BY CASE WHEN UPPER(l.[Code])='TR' THEN 0 ELSE 1 END) FROM [Collections] c;
                UPDATE s SET s.[SeoUrl]=(SELECT TOP (1) t.[Value] FROM [Translations] t INNER JOIN [Languages] l ON l.[Id]=t.[LanguageId] WHERE t.[EntityType]='Surface' AND t.[EntityId]=s.[Id] AND t.[FieldName]='SeoUrl' ORDER BY CASE WHEN UPPER(l.[Code])='TR' THEN 0 ELSE 1 END) FROM [Surfaces] s;
                DELETE FROM [Translations] WHERE [EntityType] IN ('Category','Collection','Surface');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeoUrl",
                table: "Surfaces");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "SeoUrl",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "SeoUrl",
                table: "Categories");
        }
    }
}
