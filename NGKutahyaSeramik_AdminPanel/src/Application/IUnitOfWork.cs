namespace Application;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();

    /// <summary>Excel Import (backlog #17) gibi tek bir isteğin birden fazla `SaveChangesAsync`
    /// çağrısı yapması ama hepsinin ATOMİK olması gereken senaryolar için — "hatalı dosyada kısmi
    /// kayıt bırakma" kuralı bunu gerektiriyor. Diğer tüm mevcut servisler (Product/Category/...)
    /// bu metodu hiç kullanmaz, tek `SaveChangesAsync` çağrısına güvenmeye devam eder.</summary>
    Task<IUnitOfWorkTransaction> BeginTransactionAsync();
}

public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    Task CommitAsync();

    Task RollbackAsync();
}
