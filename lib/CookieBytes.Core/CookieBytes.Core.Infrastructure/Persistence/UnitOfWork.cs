using CookieBytes.Core.Application.Abstractions;
using CookieBytes.Core.Domain.Interfaces;
using CookieBytes.Core.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CookieBytes.Core.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation with optional domain event dispatch after save.
/// </summary>
public abstract class UnitOfWork<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;

    /// <summary>
    /// Gets the database context managed by this unit of work.
    /// </summary>
    protected TDbContext DatabaseContext { get; }

    private readonly IDomainEventDispatcher? _domainEventDispatcher;

    /// <summary>
    /// Initializes a new unit of work.
    /// </summary>
    /// <param name="databaseContext">The EF Core context.</param>
    /// <param name="domainEventDispatcher">Optional dispatcher for domain events after save.</param>
    protected UnitOfWork(TDbContext databaseContext, IDomainEventDispatcher? domainEventDispatcher = null)
    {
        DatabaseContext = databaseContext;
        _domainEventDispatcher = domainEventDispatcher;
    }

    /// <inheritdoc />
    public bool IsDisposed { get; protected set; }

    /// <summary>
    /// Returns a cached repository instance, creating it via <paramref name="factory"/> on first use.
    /// </summary>
    /// <typeparam name="TRepository">The repository interface type.</typeparam>
    /// <typeparam name="TEntity">The aggregate type.</typeparam>
    /// <param name="factory">Factory that creates the repository from the context.</param>
    protected TRepository GetRepository<TRepository, TEntity>(Func<TDbContext, TRepository> factory)
        where TEntity : class, IAggregateRoot
        where TRepository : class, IRepository<TEntity>
    {
        var type = typeof(TRepository);
        if (_repositories.TryGetValue(type, out var existing))
        {
            return (TRepository)existing;
        }

        var created = factory(DatabaseContext);
        _repositories[type] = created;
        return created;
    }

    /// <inheritdoc />
    public virtual async Task<int> SaveAsync(CancellationToken cancellationToken = default) =>
        await DatabaseContext.SaveChangesAsync(cancellationToken);

    /// <inheritdoc />
    public virtual async Task<int> SaveAndDispatchEventsAsync(CancellationToken cancellationToken = default)
    {
        var aggregatesWithEvents = DatabaseContext.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        var result = await SaveAsync(cancellationToken);

        if (_domainEventDispatcher is not null && aggregatesWithEvents.Count > 0)
        {
            await _domainEventDispatcher.DispatchDomainEventsAsync(aggregatesWithEvents, cancellationToken);
        }

        return result;
    }

    /// <inheritdoc />
    public virtual Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = DatabaseContext.Database.BeginTransaction();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <inheritdoc />
    public virtual async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases managed resources when <paramref name="disposing"/> is true.
    /// </summary>
    /// <param name="disposing">Whether managed resources should be released.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            _transaction?.Dispose();
            DatabaseContext.Dispose();
        }

        IsDisposed = true;
    }

    /// <summary>
    /// Asynchronously releases managed resources.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
        }

        await DatabaseContext.DisposeAsync();
    }
}
