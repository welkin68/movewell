namespace CookieBytes.Core.Domain.Interfaces;

/// <summary>
/// Marks an entity that collects domain events for dispatch.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
