using System.Text;
using System.Text.RegularExpressions;
using CookieBytes.Core.Domain.SeedWork;
using FluentResults;
using MoveWell.Domain.Shared.Errors;

namespace MoveWell.Domain.Shared.ValueObjects;

/// <summary>URL segment shared by both languages, e.g. <c>chin-tuck</c> → /exercises/chin-tuck and /en/exercises/chin-tuck.</summary>
public sealed partial class Slug : ValueObject
{
    public const int MinLength = 3;
    public const int MaxLength = 120;

    private Slug()
    {
    }

    private Slug(string value)
    {
        Value = value;
    }

    public string Value { get; private set; } = string.Empty;

    public static Result<Slug> Create(string? value)
    {
        var candidate = value?.Trim().ToLowerInvariant() ?? string.Empty;
        return IsValid(candidate)
            ? Result.Ok(new Slug(candidate))
            : Result.Fail(DomainErrors.Slug.Invalid);
    }

    /// <summary>Builds a slug from free text (normally the English title): non-alphanumerics become single hyphens.</summary>
    public static Result<Slug> FromText(string? text)
    {
        var builder = new StringBuilder();
        foreach (var ch in (text ?? string.Empty).Trim().ToLowerInvariant())
        {
            if (ch is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                builder.Append(ch);
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var slug = builder.ToString().Trim('-');
        if (slug.Length > MaxLength)
        {
            slug = slug[..MaxLength].TrimEnd('-');
        }

        return Create(slug);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    private static bool IsValid(string candidate) =>
        candidate.Length is >= MinLength and <= MaxLength && SlugPattern().IsMatch(candidate);

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugPattern();
}
