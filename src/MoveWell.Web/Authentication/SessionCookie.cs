using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using MoveWell.Common.Contracts.Auth;

namespace MoveWell.Web.Authentication;

/// <summary>
/// Builds the cookie principal from an Api session and keeps the Api tokens inside the encrypted cookie (D-16: the browser never sees a JWT).
/// </summary>
public static class SessionCookie
{
    public const string AccessTokenName = "access_token";
    public const string RefreshTokenName = "refresh_token";
    public const string ExpiresAtName = "expires_at";
    public const string MobileClaim = "mobile";
    public const string CultureClaim = "culture";

    public static Task SignInAsync(HttpContext context, string scheme, AuthSessionResponse session)
    {
        var properties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = session.RefreshTokenExpiresAt,
            AllowRefresh = false,
        };
        StoreTokens(properties, session);
        return context.SignInAsync(scheme, CreatePrincipal(scheme, session.User), properties);
    }

    public static ClaimsPrincipal CreatePrincipal(string scheme, AuthUserResponse user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(MobileClaim, user.Mobile),
            new(CultureClaim, user.Culture),
        };
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, scheme));
    }

    public static void StoreTokens(AuthenticationProperties properties, AuthSessionResponse session)
    {
        properties.StoreTokens(
        [
            new AuthenticationToken { Name = AccessTokenName, Value = session.AccessToken },
            new AuthenticationToken { Name = RefreshTokenName, Value = session.RefreshToken },
            new AuthenticationToken { Name = ExpiresAtName, Value = session.AccessTokenExpiresAt.ToString("o", CultureInfo.InvariantCulture) },
        ]);
        properties.ExpiresUtc = session.RefreshTokenExpiresAt;
    }

    public static DateTimeOffset? AccessTokenExpiresAt(AuthenticationProperties properties) =>
        DateTimeOffset.TryParse(properties.GetTokenValue(ExpiresAtName), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value)
            ? value
            : null;
}
