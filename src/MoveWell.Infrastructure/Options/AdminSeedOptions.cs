namespace MoveWell.Infrastructure.Options;

/// <summary>
/// First staff account, created once at startup if no Admin exists.
/// Values come from user-secrets (dev) or environment variables (server) — never from committed appsettings.
/// </summary>
public sealed class AdminSeedOptions
{
    public const string SectionName = "Admin:Seed";

    public string? Mobile { get; set; }

    public string? Password { get; set; }

    public string DisplayName { get; set; } = "Admin";
}
