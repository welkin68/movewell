namespace CookieBytes.Core.Domain.Interfaces;

/// <summary>
/// Opt-in soft delete contract. Entities implementing this receive a global query filter in BaseDbContext.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }

    DateTime? DeletedAt { get; }

    string? DeletedBy { get; }

    void MarkDeleted(string deletedBy, DateTime deletedAt);
}
