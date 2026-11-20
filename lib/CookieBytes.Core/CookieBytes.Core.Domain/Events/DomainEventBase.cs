using CookieBytes.Core.Domain.Interfaces;

namespace CookieBytes.Core.Domain.Events;

/// <summary>
/// Base class for domain events with metadata.
/// </summary>
public abstract class DomainEventBase : IDomainEvent
{
    protected DomainEventBase()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }

    public Guid EventId { get; }

    public DateTime OccurredOn { get; }
}
