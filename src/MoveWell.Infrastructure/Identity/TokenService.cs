using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CookieBytes.Core.Application.Abstractions;
using FluentResults;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Common.Errors;
using MoveWell.Application.DTOs.Auth;
using MoveWell.Domain.Shared.Enumerations;
using MoveWell.Infrastructure.Options;
using MoveWell.Infrastructure.Persistence;

namespace MoveWell.Infrastructure.Identity;

/// <summary>
/// Issues JWT access tokens (HMAC-SHA256) and rotating refresh tokens (D-16).
/// Claims: sub, name, mobile, culture, role (one per role), jti.
/// </summary>
public sealed class TokenService : ITokenService
{
    public const string MobileClaim = "mobile";
    public const string CultureClaim = "culture";

    private const string RevokedRotated = "rotated";
    private const string RevokedSignOut = "signout";
    private const string RevokedReuse = "reuse_detected";
    private const string RevokedAll = "revoke_all";

    private readonly MoveWellDbContext _context;
    private readonly IIdentityService _identity;
    private readonly IDateTimeProvider _clock;
    private readonly JwtOptions _options;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        MoveWellDbContext context,
        IIdentityService identity,
        IDateTimeProvider clock,
        IOptions<JwtOptions> options,
        ILogger<TokenService> logger)
    {
        _context = context;
        _identity = identity;
        _clock = clock;
        _options = options.Value;
        _logger = logger;
    }

    public static SymmetricSecurityKey CreateSigningKey(string signingKey) => new(Encoding.UTF8.GetBytes(signingKey));

    public async Task<AuthSessionDto> IssueAsync(
        AuthUser user,
        string displayName,
        Culture culture,
        string? clientIp,
        CancellationToken cancellationToken = default)
    {
        var (raw, entity) = NewRefreshToken(user.UserId, Guid.NewGuid(), clientIp);
        _context.RefreshTokens.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return BuildSession(user, displayName, culture, raw, entity);
    }

    public async Task<Result<AuthSessionDto>> RefreshAsync(string refreshToken, string? clientIp, CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var stored = await _context.RefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == Hash(refreshToken), cancellationToken);
        if (stored is null)
        {
            return Result.Fail(AuthErrors.InvalidRefreshToken);
        }

        if (stored.RevokedAt is not null)
        {
            if (stored.ReplacedByTokenId is not null)
            {
                // A rotated token came back: someone else may hold a copy. Kill the whole family.
                _logger.LogWarning("Refresh token reuse detected for user {UserId}; revoking family {FamilyId}.", stored.UserId, stored.FamilyId);
                await RevokeWhereAsync(token => token.FamilyId == stored.FamilyId, RevokedReuse, cancellationToken);
            }

            return Result.Fail(AuthErrors.InvalidRefreshToken);
        }

        if (!stored.IsActive(now))
        {
            return Result.Fail(AuthErrors.InvalidRefreshToken);
        }

        var user = await _identity.FindByIdAsync(stored.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Fail(AuthErrors.InvalidRefreshToken);
        }

        var (raw, next) = NewRefreshToken(stored.UserId, stored.FamilyId, clientIp);
        stored.RevokedAt = now;
        stored.RevokedReason = RevokedRotated;
        stored.ReplacedByTokenId = next.Id;
        _context.RefreshTokens.Add(next);

        var profile = await _context.UserProfiles.AsNoTracking()
            .FirstOrDefaultAsync(item => item.UserId == stored.UserId, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(BuildSession(
            user,
            profile?.DisplayName ?? user.Mobile.Masked,
            profile?.PreferredCulture ?? Culture.Default,
            raw,
            next));
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hash = Hash(refreshToken);
        await RevokeWhereAsync(token => token.TokenHash == hash, RevokedSignOut, cancellationToken);
    }

    public Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default) =>
        RevokeWhereAsync(token => token.UserId == userId, RevokedAll, cancellationToken);

    private async Task RevokeWhereAsync(
        System.Linq.Expressions.Expression<Func<RefreshToken, bool>> predicate,
        string reason,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        await _context.RefreshTokens
            .Where(predicate)
            .Where(token => token.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(token => token.RevokedAt, now)
                    .SetProperty(token => token.RevokedReason, reason),
                cancellationToken);
    }

    private AuthSessionDto BuildSession(AuthUser user, string displayName, Culture culture, string rawRefresh, RefreshToken refresh)
    {
        var accessExpires = _clock.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Name, displayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(MobileClaim, user.Mobile.Value),
            new(CultureClaim, culture.Name!),
        };
        claims.AddRange(user.Roles.Select(role => new Claim("role", role)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = _clock.UtcNow,
            IssuedAt = _clock.UtcNow,
            Expires = accessExpires,
            SigningCredentials = new SigningCredentials(CreateSigningKey(_options.SigningKey), SecurityAlgorithms.HmacSha256),
        };

        var accessToken = new JsonWebTokenHandler().CreateToken(descriptor);

        return new AuthSessionDto(
            accessToken,
            accessExpires,
            rawRefresh,
            refresh.ExpiresAt,
            new AuthUserDto(user.UserId, user.Mobile.Value, displayName, culture.Name!, user.Roles));
    }

    private (string Raw, RefreshToken Entity) NewRefreshToken(Guid userId, Guid familyId, string? clientIp)
    {
        var raw = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
        var now = _clock.UtcNow;
        var entity = new RefreshToken
        {
            UserId = userId,
            FamilyId = familyId,
            TokenHash = Hash(raw),
            CreatedAt = now,
            ExpiresAt = now.AddDays(_options.RefreshTokenDays),
            CreatedByIp = clientIp is { Length: <= 45 } ? clientIp : null,
        };
        return (raw, entity);
    }

    private static string Hash(string raw) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}
