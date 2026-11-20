using System.Reflection;

namespace CookieBytes.Core.Domain.SeedWork;

/// <summary>
/// Smart enumeration base type with value/name semantics and reflection-based lookup.
/// </summary>
public abstract class Enumeration : IComparable
{
    public int Value { get; }

    public string? Name { get; }

    protected Enumeration()
    {
        Value = default;
    }

    protected Enumeration(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public static IEnumerable<T> GetAll<T>() where T : Enumeration =>
        typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => !f.IsLiteral)
            .Select(f => f.GetValue(null))
            .Cast<T>();

    public static T? FromValue<T>(int value) where T : Enumeration =>
        GetAll<T>().FirstOrDefault(item => item.Value == value);

    public static T? FromName<T>(string name) where T : Enumeration =>
        string.IsNullOrWhiteSpace(name)
            ? null
            : GetAll<T>().FirstOrDefault(item => item.Name == name);

    public static bool TryGetFromValueOrName<T>(string valueOrName, out T? enumeration) where T : Enumeration
    {
        enumeration = FromName<T>(valueOrName);
        if (enumeration is not null)
        {
            return true;
        }

        if (int.TryParse(valueOrName, out var value))
        {
            enumeration = FromValue<T>(value);
            return enumeration is not null;
        }

        enumeration = null;
        return false;
    }

    public override string ToString() => Name ?? string.Empty;

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration other)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (GetRealType() != other.GetRealType())
        {
            return false;
        }

        return Value.Equals(other.Value);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(object? obj) => Value.CompareTo(((Enumeration?)obj)?.Value);

    private Type GetRealType()
    {
        var type = GetType();
        return type.ToString().Contains("Castle.Proxies.", StringComparison.Ordinal)
            ? type.BaseType ?? type
            : type;
    }
}
