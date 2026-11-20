namespace CookieBytes.Core.Domain.Interfaces;

/// <summary>
/// Base contract for all domain entities with a typed identifier.
/// </summary>
/// <typeparam name="TId">Identifier type (for example <see cref="Guid"/> or <see cref="int"/>).</typeparam>
public interface IEntity<TId> where TId : notnull
{
    TId Id { get; }
}

/// <summary>
/// Guid-keyed entity contract (default for aggregate roots).
/// </summary>
public interface IEntity : IEntity<Guid>
{
}
