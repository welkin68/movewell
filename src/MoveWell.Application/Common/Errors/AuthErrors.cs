using MoveWell.Domain.Shared.Errors;

namespace MoveWell.Application.Common.Errors;

/// <summary>Use-case errors of the auth flows (codes are localization keys, never change once released).</summary>
public static class AuthErrors
{
    public static readonly DomainError InvalidCredentials =
        new("auth.invalid_credentials", "Mobile number or password is not correct.", ErrorKind.Unauthorized);

    public static readonly DomainError LockedOut =
        new("auth.locked_out", "Too many failed sign-in attempts. Try again later.", ErrorKind.Forbidden);

    public static readonly DomainError MobileNotConfirmed =
        new("auth.mobile_not_confirmed", "The mobile number of this account is not verified.", ErrorKind.Forbidden);

    public static readonly DomainError AlreadyRegistered =
        new("auth.already_registered", "This mobile number is already registered. Sign in or reset your password.", ErrorKind.Conflict);

    public static readonly DomainError InvalidRefreshToken =
        new("auth.invalid_refresh_token", "The session has expired. Please sign in again.", ErrorKind.Unauthorized);

    public static readonly DomainError NotSignedIn =
        new("auth.not_signed_in", "You must be signed in.", ErrorKind.Unauthorized);

    public static readonly DomainError CurrentPasswordWrong =
        new("auth.current_password_wrong", "The current password is not correct.");

    public static readonly DomainError CodeNotFound =
        new("otp.not_found", "No active verification code. Request a new one.");

    public static DomainError ResendTooSoon(int secondsLeft) =>
        new DomainError("otp.resend_too_soon", $"Wait {secondsLeft} seconds before requesting a new code.", ErrorKind.TooManyRequests)
            .WithArgument("seconds", secondsLeft);

    public static readonly DomainError TooManyCodes =
        new("otp.too_many_requests", "Too many codes requested for this number. Try again later.", ErrorKind.TooManyRequests);

    public static readonly DomainError SmsFailed =
        new("sms.send_failed", "The SMS could not be sent. Try again in a moment.");

    public static DomainError WeakPassword(string identityCode) =>
        new("password." + identityCode, "The password does not meet the requirements.");
}
