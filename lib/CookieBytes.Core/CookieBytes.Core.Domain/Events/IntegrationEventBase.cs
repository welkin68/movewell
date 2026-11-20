using CookieBytes.Core.Domain.Interfaces;

namespace CookieBytes.Core.Domain.Events;

/// <summary>
/// Base integration event with default metadata.
/// </summary>
public abstract class IntegrationEventBase : IIntegrationEvent
{
    /// <summary>
    /// Initializes event metadata with a new id, UTC timestamp, and type name.
    /// </summary>
    protected IntegrationEventBase()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        EventType = GetType().Name;
    }

    /// <inheritdoc />
    public Guid EventId { get; }

    /// <inheritdoc />
    public DateTime OccurredOn { get; }

    /// <inheritdoc />
    public string EventType { get; }
}
