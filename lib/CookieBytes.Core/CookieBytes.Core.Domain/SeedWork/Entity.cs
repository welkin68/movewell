using CookieBytes.Core.Domain.Interfaces;

namespace CookieBytes.Core.Domain.SeedWork;

/// <summary>
/// Base entity with identity, auditing, and equality semantics.
/// </summary>
/// <typeparam name="TId">Identifier type (for example <see cref="Guid"/> or <see cref="int"/>).</typeparam>
public abstract class Entity<TId> : IEntity<TId>, IAuditableEntity, IAuditStateful
    where TId : notnull
{
    private int? _requestedHashCode;

    public TId Id { get; private set; } = default!;

    public DateTime CreatedAt { get; protected set; }

    public string CreatedBy { get; protected set; } = "System";

    public DateTime? UpdatedAt { get; protected set; }

    public string UpdatedBy { get; protected set; } = "System";

    protected Entity()
    {
        if (typeof(TId) == typeof(Guid))
        {
            Id = (TId)(object)Guid.NewGuid();
        }

        CreatedAt = DateTime.UtcNow;
    }

    void IAuditStateful.SetAuditOnCreate(string createdBy, DateTime createdAt)
    {
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }

    void IAuditStateful.SetAuditOnUpdate(string updatedBy, DateTime updatedAt)
    {
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;
    }

    public bool IsTransient() => EqualityComparer<TId>.Default.Equals(Id, default!);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj is not Entity<TId> entity)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (GetRealType() != entity.GetRealType())
        {
            return false;
        }

        if (IsTransient() || entity.IsTransient())
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(Id, entity.Id);
    }

    public override int GetHashCode()
    {
        if (IsTransient())
        {
            return base.GetHashCode();
        }

        if (!_requestedHashCode.HasValue)
        {
            _requestedHashCode = Id.GetHashCode() ^ 0x1F;
        }

        return _requestedHashCode.Value;
    }

    private Type GetRealType()
    {
        var type = GetType();
        return type.ToString().Contains("Castle.Proxies.", StringComparison.Ordinal)
            ? type.BaseType ?? type
            : type;
    }
}

/// <summary>
/// Guid-keyed entity (default). New aggregates should use this unless you need integer database identity.
/// </summary>
public abstract class Entity : Entity<Guid>, IEntity
{
}
