namespace MoveWell.Infrastructure.Identity;

/// <summary>
/// Stored refresh token (only its SHA-256 hash). Tokens of one sign-in share a <see cref="FamilyId"/>;
/// each refresh rotates to a new token and marks the old one replaced.
/// Infrastructure-only record (not a domain aggregate): it is a security mechanism, not business data.
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public Guid FamilyId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? RevokedReason { get; set; }

    public Guid? ReplacedByTokenId { get; set; }

    public bool IsActive(DateTime utcNow) => RevokedAt is null && utcNow < ExpiresAt;
}
