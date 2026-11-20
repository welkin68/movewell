using CookieBytes.Core.Application.Abstractions;
using CookieBytes.Core.Domain.Interfaces;

namespace CookieBytes.Core.Infrastructure.Events;

/// <summary>
/// Enqueues integration events into the outbox store.
/// </summary>
public sealed class IntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IOutboxStore _outboxStore;

    /// <summary>
    /// Initializes the publisher with an outbox store.
    /// </summary>
    /// <param name="outboxStore">The store that persists integration events.</param>
    public IntegrationEventPublisher(IOutboxStore outboxStore)
    {
        _outboxStore = outboxStore;
    }

    /// <inheritdoc />
    public Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default) =>
        _outboxStore.AddAsync(integrationEvent, cancellationToken);
}
