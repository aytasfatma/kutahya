using Application;

namespace NGKutahyaSeramik.UnitTests.Common;

/// <summary>Gerçek EF Core SaveChanges'e ihtiyaç duymayan servis testlerinde kullanılır.</summary>
public class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task<int> SaveChangesAsync()
    {
        SaveChangesCallCount++;
        return Task.FromResult(1);
    }

    public Task<IUnitOfWorkTransaction> BeginTransactionAsync() =>
        Task.FromResult<IUnitOfWorkTransaction>(new FakeUnitOfWorkTransaction());

    private sealed class FakeUnitOfWorkTransaction : IUnitOfWorkTransaction
    {
        public Task CommitAsync() => Task.CompletedTask;

        public Task RollbackAsync() => Task.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
