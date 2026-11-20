namespace CookieBytes.Core.Domain.Interfaces;

/// <summary>
/// Aggregate root with a typed identifier and domain events.
/// </summary>
/// <typeparam name="TId">Identifier type.</typeparam>
public interface IAggregateRoot<TId> : IEntity<TId>, IHasDomainEvents
    where TId : notnull
{
}

/// <summary>
/// Guid-keyed aggregate root (default).
/// </summary>
public interface IAggregateRoot : IAggregateRoot<Guid>
{
}
