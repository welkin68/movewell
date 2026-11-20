using CookieBytes.Core.Domain.Interfaces;

namespace CookieBytes.Core.Application.Abstractions;

/// <summary>
/// Publishes integration events to the outbox store.
/// </summary>
public interface IIntegrationEventPublisher
{
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
