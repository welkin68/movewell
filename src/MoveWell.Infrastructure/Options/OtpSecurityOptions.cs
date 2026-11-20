namespace MoveWell.Infrastructure.Options;

/// <summary>Secret key for hashing OTP codes. User-secrets / environment variable only.</summary>
public sealed class OtpSecurityOptions
{
    public const string SectionName = "Otp";
    public const int MinimumSecretLength = 32;

    public string HashSecret { get; set; } = string.Empty;
}
