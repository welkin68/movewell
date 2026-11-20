using CookieBytes.Core.Domain.Interfaces;
using MediatR;

namespace CookieBytes.Core.Application.Abstractions;

/// <summary>
/// Handles domain events raised by aggregates and dispatched after save.
/// Prefer this over raw <see cref="INotificationHandler{TNotification}"/> for domain events.
/// </summary>
/// <typeparam name="TEvent">Domain event type implementing <see cref="IDomainEvent"/>.</typeparam>
public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
{
}
