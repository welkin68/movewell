using System.Linq.Expressions;

namespace CookieBytes.Core.Domain.Interfaces;

/// <summary>
/// Generic repository contract for aggregate persistence.
/// </summary>
/// <typeparam name="T">Aggregate type.</typeparam>
/// <typeparam name="TId">Identifier type.</typeparam>
public interface IRepository<T, TId>
    where T : class, IAggregateRoot<TId>
    where TId : notnull
{
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task RemoveAsync(T entity, CancellationToken cancellationToken = default);

    Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    Task<bool> RemoveByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<T?> FindByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Alias for <see cref="FindByIdAsync"/> (legacy repository API compatibility).
    /// </summary>
    Task<T?> FindAsync(TId id, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Guid-keyed repository contract (default).
/// </summary>
/// <typeparam name="T">Aggregate type.</typeparam>
public interface IRepository<T> : IRepository<T, Guid>
    where T : class, IAggregateRoot
{
}
