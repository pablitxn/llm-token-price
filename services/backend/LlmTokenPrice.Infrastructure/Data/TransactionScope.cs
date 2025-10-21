using LlmTokenPrice.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace LlmTokenPrice.Infrastructure.Data;

/// <summary>
/// Implementation of ITransactionScope for Entity Framework Core database transactions.
/// Story 2.13 Task 6: Wraps EF Core IDbContextTransaction to follow Hexagonal Architecture.
/// </summary>
/// <remarks>
/// This adapter allows the Application layer to use database transactions
/// without taking a direct dependency on Entity Framework Core.
/// Automatically rolls back on disposal if CommitAsync not called.
/// </remarks>
internal sealed class TransactionScope : ITransactionScope
{
    private readonly IDbContextTransaction _transaction;
    private bool _committed;

    public TransactionScope(IDbContextTransaction transaction)
    {
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        _committed = false;
    }

    /// <summary>
    /// Commits the transaction, persisting all changes to the database.
    /// </summary>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.CommitAsync(cancellationToken);
        _committed = true;
    }

    /// <summary>
    /// Explicitly rolls back the transaction, discarding all changes.
    /// </summary>
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.RollbackAsync(cancellationToken);
    }

    /// <summary>
    /// Disposes the transaction. Automatically rolls back if not committed.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!_committed)
        {
            // Automatic rollback if not committed
            await _transaction.RollbackAsync();
        }

        await _transaction.DisposeAsync();
    }
}
