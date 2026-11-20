using CookieBytes.Core.Domain.Interfaces;

namespace CookieBytes.Core.Domain.SeedWork;

/// <summary>
/// Entity base with soft-delete support.
/// </summary>
/// <typeparam name="TId">Identifier type.</typeparam>
public abstract class SoftDeletableEntity<TId> : Entity<TId>, ISoftDeletable
    where TId : notnull
{
    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public string? DeletedBy { get; private set; }

    public void MarkDeleted(string deletedBy, DateTime deletedAt)
    {
        IsDeleted = true;
        DeletedBy = deletedBy;
        DeletedAt = deletedAt;
    }
}

/// <summary>
/// Guid-keyed soft-deletable entity.
/// </summary>
public abstract class SoftDeletableEntity : SoftDeletableEntity<Guid>
{
}
