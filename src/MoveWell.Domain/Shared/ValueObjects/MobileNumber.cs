using System.Text;
using CookieBytes.Core.Domain.SeedWork;
using FluentResults;
using MoveWell.Domain.Shared.Errors;

namespace MoveWell.Domain.Shared.ValueObjects;

/// <summary>
/// Iranian mobile number normalized to <c>09xxxxxxxxx</c> (11 latin digits). Accepts Persian/Arabic digits,
/// spaces, dashes, parentheses and the prefixes <c>+98</c>, <c>0098</c>, <c>98</c> or a bare <c>9xxxxxxxxx</c>.
/// Used as the Identity user name and as the SMS destination (D-09).
/// </summary>
public sealed class MobileNumber : ValueObject
{
    public const int Length = 11;

    private MobileNumber()
    {
    }

    private MobileNumber(string value)
    {
        Value = value;
    }

    public string Value { get; private set; } = string.Empty;

    public static Result<MobileNumber> Create(string? input)
    {
        var digits = ExtractLatinDigits(input);
        if (digits is null)
        {
            return Result.Fail(DomainErrors.Mobile.Invalid);
        }

        var national = digits switch
        {
            { Length: 14 } when digits.StartsWith("0098", StringComparison.Ordinal) => "0" + digits[4..],
            { Length: 12 } when digits.StartsWith("98", StringComparison.Ordinal) => "0" + digits[2..],
            { Length: 10 } when digits[0] == '9' => "0" + digits,
            _ => digits,
        };

        return national.Length == Length && national.StartsWith("09", StringComparison.Ordinal)
            ? Result.Ok(new MobileNumber(national))
            : Result.Fail(DomainErrors.Mobile.Invalid);
    }

    /// <summary>Masked form for logs and UI, e.g. <c>0912***6789</c>.</summary>
    public string Masked => $"{Value[..4]}***{Value[^4..]}";

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>Returns only digits (converted to latin), or null when an unexpected character is present.</summary>
    private static string? ExtractLatinDigits(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var builder = new StringBuilder(input.Length);
        foreach (var ch in input.Trim())
        {
            switch (ch)
            {
                case >= '0' and <= '9':
                    builder.Append(ch);
                    break;
                case >= '۰' and <= '۹': // Persian digits
                    builder.Append((char)('0' + (ch - '۰')));
                    break;
                case >= '٠' and <= '٩': // Arabic-Indic digits
                    builder.Append((char)('0' + (ch - '٠')));
                    break;
                case ' ' or '-' or '(' or ')' or '‌':
                    break;
                case '+' when builder.Length == 0:
                    break;
                default:
                    return null;
            }
        }

        return builder.ToString();
    }
}
