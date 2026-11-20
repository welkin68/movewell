using CookieBytes.Core.Domain.Interfaces;

namespace CookieBytes.Core.Infrastructure.Events;

/// <summary>
/// Maps integration event type names to CLR types for outbox deserialization.
/// </summary>
public sealed class IntegrationEventTypeRegistry
{
    private readonly Dictionary<string, Type> _types = new(StringComparer.Ordinal);

    /// <summary>
    /// Registers an integration event type using its CLR type name as the key.
    /// </summary>
    /// <typeparam name="TEvent">The integration event type.</typeparam>
    public void Register<TEvent>()
        where TEvent : IIntegrationEvent
    {
        var type = typeof(TEvent);
        if (!_types.TryAdd(type.Name, type))
        {
            throw new InvalidOperationException($"Integration event type '{type.Name}' is already registered.");
        }
    }

    /// <summary>
    /// Resolves a registered integration event type by name.
    /// </summary>
    /// <param name="eventType">The event type name stored on <see cref="Application.Abstractions.OutboxMessage"/>.</param>
    /// <returns>The CLR type, or <see langword="null"/> when not registered.</returns>
    public Type? Resolve(string eventType) =>
        _types.GetValueOrDefault(eventType);
}
