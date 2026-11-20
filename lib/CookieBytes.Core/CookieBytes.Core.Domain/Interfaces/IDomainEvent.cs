using MediatR;

namespace CookieBytes.Core.Domain.Interfaces;

/// <summary>
/// Marker interface for domain events raised by aggregate roots.
/// </summary>
public interface IDomainEvent : INotification
{
}
