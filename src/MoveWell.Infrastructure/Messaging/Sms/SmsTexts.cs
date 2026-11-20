using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Shared.Enumerations;

namespace MoveWell.Infrastructure.Messaging.Sms;

/// <summary>SMS bodies in both languages (D-12). Kept short: one SMS segment.</summary>
public static class SmsTexts
{
    public static string Otp(string code, OtpPurpose purpose, Culture culture)
    {
        var isReset = purpose == OtpPurpose.PasswordReset;
        return culture == Culture.En
            ? $"MoveWell {(isReset ? "password reset" : "verification")} code: {code}\nDo not share this code."
            : $"کد {(isReset ? "بازیابی رمز" : "تأیید")} MoveWell: {code}\nاین کد را به کسی ندهید.";
    }
}
