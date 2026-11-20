using System.Text.Json;
using CookieBytes.Core.Application.Abstractions;
using CookieBytes.Core.Application.Abstractions.Modules;
using CookieBytes.Core.Domain.Interfaces;

namespace CookieBytes.Core.Infrastructure.Events;

/// <summary>
/// Deserializes outbox messages and dispatches them via <see cref="IEventBus"/> for in-process handling.
/// </summary>
/// <remarks>
/// Monolith/demo path only — production hosts typically replace this with a broker-backed
/// <see cref="IOutboxProcessor"/> implementation.
/// </remarks>
public sealed class InProcessOutboxProcessor : IOutboxProcessor
{
    private readonly IOutboxStore _outboxStore;
    private readonly IEventBus _eventBus;
    private readonly IntegrationEventTypeRegistry _eventTypeRegistry;

    /// <summary>
    /// Initializes the processor.
    /// </summary>
    /// <param name="outboxStore">The outbox store.</param>
    /// <param name="eventBus">The in-process integration event bus.</param>
    /// <param name="eventTypeRegistry">Registered integration event types.</param>
    public InProcessOutboxProcessor(
        IOutboxStore outboxStore,
        IEventBus eventBus,
        IntegrationEventTypeRegistry eventTypeRegistry)
    {
        _outboxStore = outboxStore ?? throw new ArgumentNullException(nameof(outboxStore));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _eventTypeRegistry = eventTypeRegistry ?? throw new ArgumentNullException(nameof(eventTypeRegistry));
    }

    /// <inheritdoc />
    public async Task ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _outboxStore.GetPendingAsync(cancellationToken);

        foreach (var message in pending)
        {
            try
            {
                var eventType = _eventTypeRegistry.Resolve(message.EventType);
                if (eventType is null)
                {
                    await _outboxStore.MarkFailedAsync(
                        message,
                        $"Integration event type '{message.EventType}' is not registered.",
                        cancellationToken);
                    continue;
                }

                var integrationEvent = JsonSerializer.Deserialize(message.Payload, eventType) as IIntegrationEvent;
                if (integrationEvent is null)
                {
                    await _outboxStore.MarkFailedAsync(
                        message,
                        $"Failed to deserialize outbox message '{message.Id}' as '{message.EventType}'.",
                        cancellationToken);
                    continue;
                }

                await _eventBus.PublishAsync(integrationEvent, cancellationToken);
                await _outboxStore.MarkProcessedAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                await _outboxStore.MarkFailedAsync(message, ex.Message, cancellationToken);
            }
        }
    }
}
