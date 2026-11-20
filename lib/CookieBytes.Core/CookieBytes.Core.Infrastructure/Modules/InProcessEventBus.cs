using CookieBytes.Core.Application.Abstractions.Modules;
using CookieBytes.Core.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CookieBytes.Core.Infrastructure.Modules;

/// <summary>
/// Dispatches integration events to scoped <see cref="IIntegrationEventHandler{TEvent}"/> implementations.
/// </summary>
public sealed class InProcessEventBus : IEventBus
{
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes the event bus.
    /// </summary>
    /// <param name="scopeFactory">Scope factory used to resolve handlers per publish.</param>
    public InProcessEventBus(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    /// <inheritdoc />
    public async Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        using var scope = _scopeFactory.CreateScope();
        var eventType = integrationEvent.GetType();
        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var enumerableType = typeof(IEnumerable<>).MakeGenericType(handlerType);
        var handlers = (System.Collections.IEnumerable)scope.ServiceProvider.GetRequiredService(enumerableType);

        var handleMethod = handlerType.GetMethod(nameof(IIntegrationEventHandler<IIntegrationEvent>.HandleAsync))
            ?? throw new InvalidOperationException($"HandleAsync was not found on {handlerType.Name}.");

        foreach (var handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            var task = (Task)handleMethod.Invoke(handler, [integrationEvent, cancellationToken])!;
            await task.ConfigureAwait(false);
        }
    }
}
