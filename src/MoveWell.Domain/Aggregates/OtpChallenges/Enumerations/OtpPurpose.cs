using CookieBytes.Core.Domain.SeedWork;

namespace MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;

/// <summary>Why a one-time code was sent (D-13). A code for one purpose never works for another.</summary>
public sealed class OtpPurpose : Enumeration
{
    public static readonly OtpPurpose Registration = new(1, nameof(Registration));
    public static readonly OtpPurpose PasswordReset = new(2, nameof(PasswordReset));

    private OtpPurpose(int value, string name)
        : base(value, name)
    {
    }
}
