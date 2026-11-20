namespace CookieBytes.Core.Domain.Interfaces;

/// <summary>
/// Contract for applying audit fields from infrastructure.
/// </summary>
public interface IAuditStateful
{
    void SetAuditOnCreate(string createdBy, DateTime createdAt);

    void SetAuditOnUpdate(string updatedBy, DateTime updatedAt);
}
