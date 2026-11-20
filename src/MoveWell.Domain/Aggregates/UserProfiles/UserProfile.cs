using CookieBytes.Core.Domain.SeedWork;
using FluentResults;
using MoveWell.Domain.Shared.Enumerations;
using MoveWell.Domain.Shared.Errors;

namespace MoveWell.Domain.Aggregates.UserProfiles;

/// <summary>
/// Person data for an Identity user (1:1). Identity owns credentials, phone and lockout;
/// everything about the person lives here, never on ApplicationUser (user-profile law).
/// </summary>
public class UserProfile : AggregateRoot
{
    public const int DisplayNameMinLength = 2;
    public const int DisplayNameMaxLength = 60;

    protected UserProfile()
    {
    }

    private UserProfile(Guid userId, string displayName, Culture preferredCulture, DateTime acceptedTermsAt)
    {
        UserId = userId;
        DisplayName = displayName;
        PreferredCulture = preferredCulture;
        AcceptedTermsAt = acceptedTermsAt;
    }

    public Guid UserId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public Culture PreferredCulture { get; private set; } = Culture.Default;

    /// <summary>When the user accepted the terms and the educational disclaimer (at registration).</summary>
    public DateTime AcceptedTermsAt { get; private set; }

    public static Result<UserProfile> Create(Guid userId, string? displayName, Culture preferredCulture, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(preferredCulture);

        if (userId == Guid.Empty)
        {
            return Result.Fail(DomainErrors.Profile.UserRequired);
        }

        var name = NormalizeName(displayName);
        if (name.IsFailed)
        {
            return name.ToResult<UserProfile>();
        }

        return Result.Ok(new UserProfile(userId, name.Value, preferredCulture, utcNow));
    }

    public Result Rename(string? displayName)
    {
        var name = NormalizeName(displayName);
        if (name.IsFailed)
        {
            return name.ToResult();
        }

        DisplayName = name.Value;
        return Result.Ok();
    }

    public Result ChangeCulture(Culture culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        PreferredCulture = culture;
        return Result.Ok();
    }

    private static Result<string> NormalizeName(string? displayName)
    {
        var parts = (displayName ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var name = string.Join(' ', parts);
        return name.Length is < DisplayNameMinLength or > DisplayNameMaxLength
            ? Result.Fail(DomainErrors.Profile.DisplayNameLength(DisplayNameMinLength, DisplayNameMaxLength))
            : Result.Ok(name);
    }
}
