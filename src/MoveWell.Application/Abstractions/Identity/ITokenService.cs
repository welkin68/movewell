using FluentResults;
using MoveWell.Application.DTOs.Auth;
using MoveWell.Domain.Shared.Enumerations;

namespace MoveWell.Application.Abstractions.Identity;

/// <summary>
/// JWT access tokens + rotating refresh tokens (D-16, ADR-0003). Refresh tokens are stored hashed;
/// reusing a rotated token revokes its whole family (stolen-token detection).
/// </summary>
public interface ITokenService
{
    Task<AuthSessionDto> IssueAsync(
        AuthUser user,
        string displayName,
        Culture culture,
        string? clientIp,
        CancellationToken cancellationToken = default);

    Task<Result<AuthSessionDto>> RefreshAsync(string refreshToken, string? clientIp, CancellationToken cancellationToken = default);

    /// <summary>Revokes one refresh token (sign-out). Unknown tokens are ignored.</summary>
    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Revokes every active refresh token of a user (password reset/change).</summary>
    Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default);
}
