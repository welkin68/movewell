using CookieBytes.Core.Domain.Interfaces;

namespace CookieBytes.Core.Domain.SeedWork;

/// <summary>
/// Entity base with optimistic concurrency via row version token.
/// </summary>
/// <typeparam name="TId">Identifier type.</typeparam>
public abstract class RowVersionedEntity<TId> : Entity<TId>, IRowVersioned
    where TId : notnull
{
    public byte[] RowVersion { get; private set; } = [];

    public virtual void SetRowVersion(byte[] rowVersion)
    {
        RowVersion = rowVersion ?? [];
    }
}

/// <summary>
/// Guid-keyed row-versioned entity.
/// </summary>
public abstract class RowVersionedEntity : RowVersionedEntity<Guid>
{
}
