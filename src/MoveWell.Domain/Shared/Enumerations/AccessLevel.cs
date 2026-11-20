using CookieBytes.Core.Domain.SeedWork;

namespace MoveWell.Domain.Shared.Enumerations;

/// <summary>Who may see content. Everything is <see cref="Free"/> at launch (D-03); Premium waits for the subscription gate.</summary>
public sealed class AccessLevel : Enumeration
{
    public static readonly AccessLevel Free = new(1, nameof(Free));
    public static readonly AccessLevel Premium = new(2, nameof(Premium));

    private AccessLevel(int value, string name)
        : base(value, name)
    {
    }
}
