namespace CookieBytes.Core.Domain.Interfaces;

/// <summary>
/// Cross-boundary integration event contract (distinct from in-process domain events).
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTime OccurredOn { get; }

    string EventType { get; }
}
