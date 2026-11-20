using System.Text.Json.Serialization;
using CookieBytes.Core.Domain.Interfaces;

namespace CookieBytes.Core.Domain.SeedWork;

/// <summary>
/// Base aggregate root with domain event support.
/// </summary>
/// <typeparam name="TId">Identifier type.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    [JsonIgnore]
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    protected AggregateRoot()
    {
    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is not null)
        {
            _domainEvents.Add(domainEvent);
        }
    }

    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is not null)
        {
            _domainEvents.Remove(domainEvent);
        }
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Override to validate aggregate state during <c>Create</c> factories.
    /// </summary>
    protected virtual void OnValidate()
    {
    }

    /// <summary>
    /// Override for logic that runs after construction but before persistence.
    /// </summary>
    protected virtual void OnBeforeCreate()
    {
    }

    /// <summary>
    /// Override for logic that runs after the aggregate is fully initialized.
    /// </summary>
    protected virtual void OnAfterCreate()
    {
    }
}

/// <summary>
/// Guid-keyed aggregate root (default).
/// </summary>
public abstract class AggregateRoot : AggregateRoot<Guid>, IAggregateRoot
{
}
