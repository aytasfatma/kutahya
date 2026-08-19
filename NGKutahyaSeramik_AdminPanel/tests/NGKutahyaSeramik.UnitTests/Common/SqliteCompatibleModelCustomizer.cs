using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace NGKutahyaSeramik.UnitTests.Common;

/// <summary>
/// Üretim şeması SQL Server'a özgü ham kolon tipi string'leri içeriyor (ör. `Translation.Value`
/// için `HasColumnType("nvarchar(max)")`) — SQLite bu literal tip adını tanımıyor ve `CREATE TABLE`
/// DDL'i sözdizimi hatasıyla patlıyor. Bu `IModelCustomizer`, gerçek `AppDbContext.OnModelCreating`'i
/// (tüm FK/index/constraint/DeleteBehavior kuralları dahil) olduğu gibi çalıştırdıktan SONRA yalnızca
/// SQLite'ın anlamadığı ham "nvarchar(max)" tip adını temizler; EF, SQLite için kendi varsayılan
/// (TEXT) eşlemesini seçer. Üretim kodu/migration'ları DEĞİŞTİRİLMEZ — bu yalnızca test sürecinde
/// SQLite sağlayıcısı için `DbContextOptionsBuilder.ReplaceService&lt;IModelCustomizer, _&gt;()` ile
/// devreye alınan, bellek-içi model üzerinde uygulanan bir düzeltmedir.
///
/// Ayrıca üretim SQL Server veritabanının gerçek collation'ı `SQL_Latin1_General_CP1_CI_AS`
/// (case-insensitive, sqlcmd ile doğrulandı) — SQLite'ın varsayılanı ise case-SENSITIVE'dir.
/// Model'e "NOCASE" varsayılan collation'ı uygulanarak (yalnızca test modelinde; üretim
/// konfigürasyonu değişmez) Tag.Name/ProductCode gibi unique index'lerin case-insensitive
/// tekilleştirme davranışı testte de gerçekçi şekilde sınanabilir.
/// </summary>
public class SqliteCompatibleModelCustomizer : ModelCustomizer
{
    public SqliteCompatibleModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
    {
    }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (string.Equals(property.GetColumnType(), "nvarchar(max)", StringComparison.OrdinalIgnoreCase))
                {
                    property.SetColumnType(null);
                }

                if (property.ClrType == typeof(string))
                {
                    property.SetCollation("NOCASE");
                }
            }
        }
    }
}
