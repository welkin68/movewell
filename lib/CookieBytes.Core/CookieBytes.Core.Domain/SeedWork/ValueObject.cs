using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace CookieBytes.Core.Domain.SeedWork;

/// <summary>
/// Base type for immutable value objects with component-based equality.
/// </summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public static bool operator ==(ValueObject? left, ValueObject? right)
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

    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType() || obj is not ValueObject other)
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        using var sha = SHA256.Create();
        var values = GetEqualityComponents().Select(x => x?.ToString() ?? string.Empty);
        var joined = string.Join("|", values);
        return BitConverter.ToInt32(sha.ComputeHash(Encoding.UTF8.GetBytes(joined)), 0);
    }
}
