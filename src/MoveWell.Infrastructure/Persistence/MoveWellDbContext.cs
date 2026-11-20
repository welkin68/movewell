using System.Linq.Expressions;
using CookieBytes.Core.Application.Abstractions;
using CookieBytes.Core.Domain.Interfaces;
using CookieBytes.Core.Domain.SeedWork;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoveWell.Domain.Aggregates.OtpChallenges;
using MoveWell.Domain.Aggregates.UserProfiles;
using MoveWell.Infrastructure.Identity;

namespace MoveWell.Infrastructure.Persistence;

/// <summary>
/// One database for Identity tables and domain aggregates.
/// C# allows one base class, so this inherits IdentityDbContext and re-applies the CookieBytes.Core
/// conventions that BaseDbContext would give: audit on save, soft-delete filters, ignored DomainEvents (same as yildiz).
/// </summary>
public sealed class MoveWellDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly ICurrentUserService? _currentUserService;
    private readonly IDateTimeProvider? _dateTimeProvider;

    public MoveWellDbContext(
        DbContextOptions<MoveWellDbContext> options,
        ICurrentUserService? currentUserService = null,
        IDateTimeProvider? dateTimeProvider = null)
        : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<OtpChallenge> OtpChallenges => Set<OtpChallenge>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAudit();
        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MoveWellDbContext).Assembly);

        ApplySoftDeleteFilters(modelBuilder);
        IgnoreDomainEvents(modelBuilder);
    }

    private void ApplyAudit()
    {
        var userId = _currentUserService?.UserId ?? "System";
        var utcNow = _dateTimeProvider?.UtcNow ?? DateTime.UtcNow;

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

    private static void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType) || entityType.IsOwned())
            {
                continue;
            }

            modelBuilder.Entity(entityType.ClrType)
                .Property<bool>(nameof(ISoftDeletable.IsDeleted))
                .HasDefaultValue(false);

            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(false)), parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    private static void IgnoreDomainEvents(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasDomainEvents).IsAssignableFrom(entityType.ClrType) && !entityType.IsOwned())
            {
                modelBuilder.Entity(entityType.ClrType).Ignore(nameof(AggregateRoot.DomainEvents));
            }
        }
    }
}
