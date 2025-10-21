namespace LlmTokenPrice.Domain.Repositories;

/// <summary>
/// Interface (port) for database transaction management in Hexagonal Architecture.
/// Story 2.13 Task 6: Enables Application layer to use transactions without depending on Infrastructure.
/// </summary>
/// <remarks>
/// This interface allows the Application layer to manage database transactions
/// without taking a direct dependency on Entity Framework Core or AppDbContext.
/// Follows Dependency Inversion Principle: Infrastructure implements this port.
/// </remarks>
public interface ITransactionScope : IAsyncDisposable
{
    /// <summary>
    /// Commits the current transaction, persisting all changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <remarks>
    /// Call this method only after all operations within the transaction have succeeded.
    /// If this method is not called before disposal, the transaction will be rolled back automatically.
    /// </remarks>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Explicitly rolls back the current transaction, discarding all changes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <remarks>
    /// Call this method to explicitly roll back changes when an error occurs.
    /// Transaction will also be rolled back automatically if not committed before disposal.
    /// </remarks>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
