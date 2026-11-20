namespace CookieBytes.Core.Domain.Interfaces;

/// <summary>
/// Audit metadata contract implemented by <see cref="SeedWork.Entity"/>.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; }

    string CreatedBy { get; }

    DateTime? UpdatedAt { get; }

    string UpdatedBy { get; }
}
