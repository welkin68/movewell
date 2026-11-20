namespace MoveWell.Domain.Shared.Errors;

/// <summary>Catalogue of every domain error code. Codes never change once released (clients may rely on them).</summary>
public static class DomainErrors
{
    public static class Text
    {
        public static readonly DomainError FaRequired =
            new("text.fa_required", "Persian text is required.");

        public static DomainError TooLong(int maxLength) =>
            new DomainError("text.too_long", $"Text must be at most {maxLength} characters.").WithArgument("max", maxLength);
    }

    public static class Slug
    {
        public static readonly DomainError Invalid =
            new("slug.invalid", "Slug must be 3-120 characters of lowercase latin letters, digits and single hyphens.");
    }

    public static class Mobile
    {
        public static readonly DomainError Invalid =
            new("mobile.invalid", "Mobile number must be an Iranian mobile number (09xxxxxxxxx).");
    }

    public static class Otp
    {
        public static readonly DomainError Invalid =
            new("otp.invalid", "The verification code is not correct.");

        public static readonly DomainError Expired =
            new("otp.expired", "The verification code has expired. Request a new one.");

        public static readonly DomainError TooManyAttempts =
            new("otp.too_many_attempts", "Too many wrong attempts. Request a new code.");

        public static readonly DomainError AlreadyUsed =
            new("otp.already_used", "This verification code was already used.");

        public static readonly DomainError NotVerified =
            new("otp.not_verified", "The code must be verified before it can be consumed.");

        public static readonly DomainError InvalidSettings =
            new("otp.invalid_settings", "OTP lifetime and attempt limit must be positive.");
    }

    public static class Profile
    {
        public static DomainError DisplayNameLength(int min, int max) =>
            new DomainError("profile.display_name_length", $"Display name must be {min}-{max} characters.")
                .WithArgument("min", min)
                .WithArgument("max", max);

        public static readonly DomainError UserRequired =
            new("profile.user_required", "A profile must belong to a user.");
    }
}
