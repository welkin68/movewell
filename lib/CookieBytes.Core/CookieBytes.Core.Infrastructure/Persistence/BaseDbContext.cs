using System.Linq.Expressions;
using CookieBytes.Core.Application.Abstractions;
using CookieBytes.Core.Domain.Interfaces;
using CookieBytes.Core.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace CookieBytes.Core.Infrastructure.Persistence;

/// <summary>
/// Base DbContext with table prefix, soft-delete filters, and audit support.
/// </summary>
public abstract class BaseDbContext : DbContext
{
    private readonly string? _tablePrefix;

    /// <summary>
    /// Initializes the context with optional audit services and table prefix.
    /// </summary>
    /// <param name="options">DbContext options.</param>
    /// <param name="currentUserService">Optional service supplying the current user id for auditing.</param>
    /// <param name="dateTimeProvider">Optional service supplying UTC timestamps for auditing.</param>
    /// <param name="tablePrefix">Optional prefix applied to all table names.</param>
    protected BaseDbContext(
        DbContextOptions options,
        ICurrentUserService? currentUserService = null,
        IDateTimeProvider? dateTimeProvider = null,
        string? tablePrefix = null)
        : base(options)
    {
        _tablePrefix = tablePrefix;
        CurrentUserService = currentUserService;
        DateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Gets the current user service used for audit fields.
    /// </summary>
    protected ICurrentUserService? CurrentUserService { get; }

    /// <summary>
    /// Gets the date/time provider used for audit fields.
    /// </summary>
    protected IDateTimeProvider? DateTimeProvider { get; }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override int SaveChanges()
    {
        ApplyAudit();
        return base.SaveChanges();
    }

    /// <summary>
    /// Applies audit fields when at least one audit port is injected.
    /// If only one port is supplied, the other falls back to <c>"System"</c> / <see cref="DateTime.UtcNow"/>.
    /// </summary>
    private void ApplyAudit()
    {
        if (CurrentUserService is null && DateTimeProvider is null)
        {
            return;
        }

        var userId = CurrentUserService?.UserId ?? "System";
        var utcNow = DateTimeProvider?.UtcNow ?? DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not IAuditStateful auditable)
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    auditable.SetAuditOnCreate(userId, utcNow);
                    break;
                case EntityState.Modified:
                    auditable.SetAuditOnUpdate(userId, utcNow);
                    break;
            }
        }
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ApplyCorePersistenceConventions(modelBuilder);
    }

    /// <summary>
    /// Applies Core persistence conventions: table prefix, soft-delete filters, and domain-event ignores.
    /// Module DbContexts should call <c>ApplyConfiguration*</c> first, then <c>base.OnModelCreating</c>
    /// (which invokes this method).
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected void ApplyCorePersistenceConventions(ModelBuilder modelBuilder)
    {
        ApplyTablePrefix(modelBuilder);
        ApplySoftDeleteFilters(modelBuilder);
        IgnoreDomainEvents(modelBuilder);
    }

    private void ApplyTablePrefix(ModelBuilder modelBuilder)
    {
        if (string.IsNullOrWhiteSpace(_tablePrefix))
        {
            return;
        }

        var renames = modelBuilder.Model.GetEntityTypes()
            .Select(entityType => entityType.GetTableName())
            .Where(tableName => !string.IsNullOrWhiteSpace(tableName))
            .Distinct(StringComparer.Ordinal)
            .Where(tableName => !tableName!.StartsWith(_tablePrefix, StringComparison.Ordinal))
            .ToDictionary(tableName => tableName!, tableName => $"{_tablePrefix}{tableName}");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (tableName != null && renames.TryGetValue(tableName, out var prefixed))
            {
                entityType.SetTableName(prefixed);
            }
        }
    }

    private static void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            modelBuilder.Entity(entityType.ClrType)
                .Property<bool>(nameof(ISoftDeletable.IsDeleted))
                .HasDefaultValue(false);

            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            var filter = Expression.Lambda(
                Expression.Equal(property, Expression.Constant(false)),
                parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    private static void IgnoreDomainEvents(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasDomainEvents).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Ignore(nameof(AggregateRoot.DomainEvents));
            }
        }
    }
}
