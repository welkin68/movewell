using CookieBytes.Core.Domain.Interfaces;

namespace CookieBytes.Core.Application.Abstractions.Modules;

/// <summary>
/// In-process integration event bus for cross-module communication within a host.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an integration event to in-process handlers.
    /// </summary>
    /// <param name="integrationEvent">The event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
