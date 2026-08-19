using Application;
using Application.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsSortOrderUniqueViolation(ex))
        {
            throw new SortOrderConflictException();
        }
    }

    private static bool IsSortOrderUniqueViolation(DbUpdateException exception)
    {
        var sqlException = exception.GetBaseException() as SqlException;
        if (sqlException is null || sqlException.Number is not (2601 or 2627))
        {
            return false;
        }

        var message = sqlException.Message;
        return message.Contains("UX_Categories_ParentCategoryId_DisplayOrder", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("UX_Categories_Root_DisplayOrder", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("UX_Collections_DisplayOrder", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("UX_Products_DisplayOrder", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("UX_Banners_DisplayOrder", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("UX_BlogCategories_DisplayOrder", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("UX_NewsCategories_DisplayOrder", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("UX_Documents_DocumentType_LanguageId_DisplayOrder", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("UX_ReferenceProjects_DisplayOrder", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("UX_Languages_DisplayOrder", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync()
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync();
        return new EfCoreUnitOfWorkTransaction(transaction);
    }

    private sealed class EfCoreUnitOfWorkTransaction : IUnitOfWorkTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public EfCoreUnitOfWorkTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync() => _transaction.CommitAsync();

        public Task RollbackAsync() => _transaction.RollbackAsync();

        public ValueTask DisposeAsync() => _transaction.DisposeAsync();
    }
}
