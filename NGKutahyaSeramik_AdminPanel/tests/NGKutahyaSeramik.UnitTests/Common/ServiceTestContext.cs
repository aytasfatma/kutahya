using Application;
using Infrastructure.Persistence;
using NGKutahyaSeramik.UnitTests.Mocks;

namespace NGKutahyaSeramik.UnitTests.Common;

/// <summary>
/// Servis testleri için tek-durak kurulum: gerçek SQLite in-memory DB'ye bağlı gerçek repository'ler
/// (EF Core'un Id atama/FK/unique-constraint davranışını gerçekten sergiler — bu servislerin "Create"
/// akışı entity Id'sinin SaveChanges sonrası dolmasına bağımlı olduğu için salt Moq ile anlamlı test
/// edilemez), artı bellek-içi FakeTranslationService/FakeFileStorageService (disk/DB'ye ihtiyaç
/// duymayan, hızlı, gözlemlenebilir sahteler). "Sociable unit test" yaklaşımı: yalnızca gerçekten
/// dış/yavaş sınır (disk I/O) sahtelenir, entity/DB davranışı gerçek kalır.
/// </summary>
public sealed class ServiceTestContext : IDisposable
{
    public SqliteTestDatabase Database { get; }

    public Infrastructure.Persistence.AppDbContext DbContext => Database.Context;

    public FakeTranslationService Translations { get; }

    public FakeFileStorageService FileStorage { get; }

    public IUnitOfWork UnitOfWork { get; }

    public ServiceTestContext()
    {
        Database = SqliteTestDatabase.Create();
        Translations = new FakeTranslationService();
        FileStorage = new FakeFileStorageService();
        UnitOfWork = new UnitOfWork(DbContext);
    }

    public void Dispose() => Database.Dispose();
}
