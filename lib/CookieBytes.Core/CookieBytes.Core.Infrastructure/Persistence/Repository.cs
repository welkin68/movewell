using System.Linq.Expressions;
using CookieBytes.Core.Application.Specifications;
using CookieBytes.Core.Domain.Interfaces;
using CookieBytes.Core.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace CookieBytes.Core.Infrastructure.Persistence;

/// <summary>
/// Generic repository implementation providing CRUD operations for aggregates.
/// </summary>
/// <typeparam name="TEntity">Aggregate type.</typeparam>
/// <typeparam name="TId">Identifier type.</typeparam>
public abstract class Repository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : class, IAggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets the EF Core set for the aggregate type.
    /// </summary>
    protected DbSet<TEntity> DbSet { get; }

    /// <summary>
    /// Gets the database context backing this repository.
    /// </summary>
    protected DbContext DatabaseContext { get; }

    /// <summary>
    /// Initializes a new repository for the given database context.
    /// </summary>
    /// <param name="databaseContext">The EF Core context.</param>
    protected Repository(DbContext databaseContext)
    {
        DatabaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        DbSet = DatabaseContext.Set<TEntity>();
    }

    /// <summary>
    /// Gets a queryable over the aggregate set.
    /// </summary>
    protected IQueryable<TEntity> Query => DbSet.AsQueryable();

    /// <summary>
    /// Gets a queryable that bypasses global query filters (for example soft-delete filters).
    /// </summary>
    protected IQueryable<TEntity> QueryIncludingDeleted => DbSet.IgnoreQueryFilters();

    /// <inheritdoc />
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await DbSet.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var entry = DatabaseContext.Entry(entity);
        if (entry.State != EntityState.Modified)
        {
            entry.State = EntityState.Modified;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual async Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        DbSet.RemoveRange(entities);
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual async Task<bool> RemoveByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await FindByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        await RemoveAsync(entity, cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.ToListAsync(cancellationToken);

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await DbSet.Where(predicate).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public virtual Task<TEntity?> FindByIdAsync(TId id, CancellationToken cancellationToken = default) =>
        DbSet.FindAsync([id], cancellationToken).AsTask();

    /// <inheritdoc />
    public virtual Task<TEntity?> FindAsync(TId id, CancellationToken cancellationToken = default) =>
        FindByIdAsync(id, cancellationToken);

    /// <inheritdoc />
    public virtual Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(predicate, cancellationToken);

    /// <inheritdoc />
    public virtual Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default) =>
        predicate is null
            ? DbSet.CountAsync(cancellationToken)
            : DbSet.CountAsync(predicate, cancellationToken);

    /// <summary>
    /// Returns entities matching the given specification.
    /// </summary>
    /// <param name="specification">The query specification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    protected async Task<IReadOnlyList<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        var results = await SpecificationEvaluator
            .Apply(Query, specification)
            .ToListAsync(cancellationToken);

        return results;
    }
}

/// <summary>
/// Guid-keyed repository base (default).
/// </summary>
/// <typeparam name="TEntity">Aggregate type.</typeparam>
public abstract class Repository<TEntity> : Repository<TEntity, Guid>, IRepository<TEntity>
    where TEntity : class, IAggregateRoot
{
    /// <summary>
    /// Initializes a new repository for the given database context.
    /// </summary>
    /// <param name="databaseContext">The EF Core context.</param>
    protected Repository(DbContext databaseContext)
        : base(databaseContext)
    {
    }
}
