using CookieBytes.Core.Application.Abstractions;
using CookieBytes.Core.Domain.SeedWork;
using MediatR;

namespace CookieBytes.Core.Infrastructure.Events;

/// <summary>
/// Dispatches domain events through MediatR and clears them from aggregates.
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes the dispatcher with a MediatR mediator.
    /// </summary>
    /// <param name="mediator">The MediatR mediator used to publish events.</param>
    public DomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <inheritdoc />
    public async Task DispatchDomainEventsAsync(
        IEnumerable<AggregateRoot> aggregates,
        CancellationToken cancellationToken = default)
    {
        foreach (var aggregate in aggregates)
        {
            var events = aggregate.DomainEvents.ToList();

            foreach (var domainEvent in events)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }

            aggregate.ClearDomainEvents();
        }
    }
}
