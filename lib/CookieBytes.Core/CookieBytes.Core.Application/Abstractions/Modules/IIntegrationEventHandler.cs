using CookieBytes.Core.Domain.Interfaces;

namespace CookieBytes.Core.Application.Abstractions.Modules;

/// <summary>
/// Handles in-process integration events dispatched by <see cref="IEventBus"/>.
/// </summary>
/// <typeparam name="TEvent">The integration event type.</typeparam>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IIntegrationEvent
{
    /// <summary>
    /// Handles the integration event.
    /// </summary>
    /// <param name="event">The event instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
