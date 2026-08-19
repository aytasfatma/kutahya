using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDealerBrands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOT: dotnet ef migrations add tarafından ayrıca tespit edilen Banners.Brand (drop) ve
            // Documents.Brand (add) işlemleri bilinçli olarak çıkarıldı — bu tabloların canlı
            // veritabanındaki hali zaten mevcut entity modeliyle eşleşiyordu (önceki bir migration
            // geçmişi/snapshot tutarsızlığından kaynaklanan, bu değişiklikle ilgisiz bir drift).
            // Bu migration yalnızca Dealers.BrandCodes eklemesinden sorumlu.
            migrationBuilder.AddColumn<string>(
                name: "BrandCodes",
                table: "Dealers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "NgSeramik,NgStone,NgSlim,NgPerforma");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrandCodes",
                table: "Dealers");
        }
    }
}
