using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CleanLegacySurfaceCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @LegacyCategoryIds TABLE ([Id] int PRIMARY KEY);

                INSERT INTO @LegacyCategoryIds ([Id])
                SELECT DISTINCT c.[Id]
                FROM [Categories] c
                INNER JOIN [Translations] t
                    ON t.[EntityType] = 'Category'
                   AND t.[EntityId] = c.[Id]
                   AND t.[FieldName] = 'Name'
                WHERE EXISTS
                (
                    SELECT 1 FROM [Surfaces] s
                    WHERE UPPER(LTRIM(RTRIM(s.[Name]))) = UPPER(LTRIM(RTRIM(t.[Value])))
                )
                OR UPPER(LTRIM(RTRIM(t.[Value]))) IN
                (
                    N'HG', N'KARİSTAL', N'KRİSTAL DM', N'LAPPATO SD', N'MAT SL',
                    N'NANO DF', N'NANO DM', N'NATURAL', N'SATİNATO', N'SATİNATO DF', N'SATİNATO DM'
                );

                DECLARE @UncategorizedId int =
                (
                    SELECT TOP (1) c.[Id]
                    FROM [Categories] c
                    INNER JOIN [Translations] t
                        ON t.[EntityType] = 'Category'
                       AND t.[EntityId] = c.[Id]
                       AND t.[FieldName] = 'Name'
                    WHERE UPPER(LTRIM(RTRIM(t.[Value]))) = UPPER(N'Kategorisiz')
                );

                IF @UncategorizedId IS NULL
                    THROW 51000, 'Kategorisiz kategori bulunamadığı için eski yüzey kategorileri güvenle temizlenemedi.', 1;

                UPDATE [Products]
                SET [CategoryId] = @UncategorizedId
                WHERE [CategoryId] IN (SELECT [Id] FROM @LegacyCategoryIds);

                DELETE FROM [Translations]
                WHERE [EntityType] = 'Category'
                  AND [EntityId] IN (SELECT [Id] FROM @LegacyCategoryIds);

                DELETE FROM [Categories]
                WHERE [Id] IN (SELECT [Id] FROM @LegacyCategoryIds);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Temizlenen kayıtlar eski ve hatalı yüzey kopyalarıdır. Down sırasında yeniden
            // oluşturulmaları kategori/yüzey ayrımını tekrar bozacağı için bilinçli olarak geri eklenmez.
        }
    }
}
