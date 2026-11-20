using System.ComponentModel.DataAnnotations;

namespace MoveWell.Application.Options;

/// <summary>OTP rules (backend-guide §4.5). Defaults are the agreed values; override in config only with a reason.</summary>
public sealed class OtpOptions
{
    public const string SectionName = "Otp";

    [Range(4, 8)]
    public int CodeLength { get; set; } = 6;

    [Range(30, 900)]
    public int LifetimeSeconds { get; set; } = 120;

    [Range(1, 10)]
    public int MaxAttempts { get; set; } = 5;

    /// <summary>Minimum wait between two codes to the same mobile.</summary>
    [Range(30, 900)]
    public int ResendIntervalSeconds { get; set; } = 120;

    [Range(1, 50)]
    public int MaxPerHourPerMobile { get; set; } = 5;
}
