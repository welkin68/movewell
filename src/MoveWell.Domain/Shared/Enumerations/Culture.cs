using CookieBytes.Core.Domain.SeedWork;

namespace MoveWell.Domain.Shared.Enumerations;

/// <summary>Supported content/UI languages (D-12). Fa is the default.</summary>
public sealed class Culture : Enumeration
{
    public static readonly Culture Fa = new(1, "fa", "fa-IR");
    public static readonly Culture En = new(2, "en", "en-US");

    private Culture(int value, string name, string cultureName)
        : base(value, name)
    {
        CultureName = cultureName;
    }

    /// <summary>.NET culture name, e.g. <c>fa-IR</c>.</summary>
    public string CultureName { get; }

    public static Culture Default => Fa;

    /// <summary>Maps "fa", "fa-IR", "en", "en-US" (case-insensitive); anything else falls back to <see cref="Default"/>.</summary>
    public static Culture FromCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Default;
        }

        var twoLetter = code.Trim().Split('-')[0].ToLowerInvariant();
        return GetAll<Culture>().FirstOrDefault(c => c.Name == twoLetter) ?? Default;
    }
}
