using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace NGKutahyaSeramik.UnitTests.Common;

/// <summary>
/// EF Core ilişkileri, FK/cascade davranışları ve unique index'leri gerçekten sınayan testler için.
/// `UseInMemoryDatabase` KULLANILMAZ — o sağlayıcı FK/unique constraint'leri uygulamaz, bu yüzden
/// yanlış-pozitif "geçti" sonucu verir. Bunun yerine gerçek ilişkisel davranış sergileyen SQLite
/// in-memory bağlantısı kullanılır (bağlantı açık kaldığı sürece ":memory:" DB yaşar).
/// Her çağrı yeni, izole bir bağlantı/veritabanı açar — testler arası veri sızıntısı olmaz.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContext Context { get; }

    private SqliteTestDatabase(SqliteConnection connection, AppDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public static SqliteTestDatabase Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var context = new AppDbContext(BuildOptions(connection));
        context.Database.EnsureCreated();

        return new SqliteTestDatabase(connection, context);
    }

    /// <summary>Aynı bağlantı üzerinde temiz bir DbContext örneği (değişiklik izleyicisi sıfırlanmış).</summary>
    public AppDbContext NewContext() => new(BuildOptions(_connection));

    private static DbContextOptions<AppDbContext> BuildOptions(SqliteConnection connection) =>
        new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .ReplaceService<IModelCustomizer, SqliteCompatibleModelCustomizer>()
            .Options;

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
