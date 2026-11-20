using CookieBytes.Core.Domain.SeedWork;

namespace CookieBytes.Core.Application.Abstractions;

/// <summary>
/// Dispatches domain events from aggregate roots via MediatR.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchDomainEventsAsync(
        IEnumerable<AggregateRoot> aggregates,
        CancellationToken cancellationToken = default);
}
