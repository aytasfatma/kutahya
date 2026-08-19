using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260816183500_NormalizeSalesPointCountryAndCity")]
public partial class NormalizeSalesPointCountryAndCity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE Dealers
            SET District = City,
                City = N'Türkiye'
            WHERE Category = N'SalesPoint'
              AND City <> N'Türkiye';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Eski ilçe değeri serbest metin adresinde korunur; otomatik ve güvenilir
        // biçimde ayrıştırılamayacağı için veri dönüşümü geri çevrilmez.
    }
}
