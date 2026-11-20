namespace MoveWell.Infrastructure.Options;

/// <summary>JWT settings (ADR-0003). <see cref="SigningKey"/> is a secret: user-secrets / environment variable only.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinimumKeyLength = 32;

    public string Issuer { get; set; } = "MoveWell";

    public string Audience { get; set; } = "MoveWell.Clients";

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 30;
}
