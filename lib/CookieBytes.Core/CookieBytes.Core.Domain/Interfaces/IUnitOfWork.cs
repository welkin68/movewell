namespace CookieBytes.Core.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern for managing aggregate persistence and transactions.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    bool IsDisposed { get; }

    Task<int> SaveAsync(CancellationToken cancellationToken = default);

    Task<int> SaveAndDispatchEventsAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}
