using CookieBytes.Core.Domain.SeedWork;
using FluentResults;
using MoveWell.Domain.Shared.Enumerations;
using MoveWell.Domain.Shared.Errors;

namespace MoveWell.Domain.Shared.ValueObjects;

/// <summary>
/// A user-facing text in Persian and English (D-12). Persian is the source language;
/// English may be empty while drafting, and <see cref="IsComplete"/> tells publish rules whether both exist.
/// </summary>
public sealed class LocalizedText : ValueObject
{
    private LocalizedText()
    {
    }

    private LocalizedText(string fa, string en)
    {
        Fa = fa;
        En = en;
    }

    public string Fa { get; private set; } = string.Empty;

    public string En { get; private set; } = string.Empty;

    public static LocalizedText Empty { get; } = new(string.Empty, string.Empty);

    /// <summary>True when both languages have text.</summary>
    public bool IsComplete => Fa.Length > 0 && En.Length > 0;

    public bool IsEmpty => Fa.Length == 0 && En.Length == 0;

    /// <param name="required">When true, Persian must not be empty.</param>
    public static Result<LocalizedText> Create(string? fa, string? en, int maxLength, bool required)
    {
        var faText = Normalize(fa);
        var enText = Normalize(en);

        if (required && faText.Length == 0)
        {
            return Result.Fail(DomainErrors.Text.FaRequired);
        }

        if (faText.Length > maxLength || enText.Length > maxLength)
        {
            return Result.Fail(DomainErrors.Text.TooLong(maxLength));
        }

        return Result.Ok(new LocalizedText(faText, enText));
    }

    /// <summary>Text for <paramref name="culture"/>; English falls back to Persian when missing.</summary>
    public string For(Culture culture) =>
        culture == Culture.En && En.Length > 0 ? En : Fa;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Fa;
        yield return En;
    }

    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().Replace("\r\n", "\n", StringComparison.Ordinal);
}
