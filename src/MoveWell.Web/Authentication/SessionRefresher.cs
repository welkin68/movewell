using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MoveWell.Web.Services.Auth;

namespace MoveWell.Web.Authentication;

/// <summary>
/// Runs on every request that carries a session cookie. When the 15-minute access token is about to expire,
/// it exchanges the refresh token at the Api and rewrites the cookie; if the Api refuses (revoked, expired, stolen),
/// the cookie is dropped and the user is simply signed out.
/// </summary>
public sealed class SessionRefresher : CookieAuthenticationEvents
{
    private static readonly TimeSpan RefreshMargin = TimeSpan.FromSeconds(60);

    private readonly IAuthApiClient _authApi;
    private readonly ILogger<SessionRefresher> _logger;

    public SessionRefresher(IAuthApiClient authApi, ILogger<SessionRefresher> logger)
    {
        _authApi = authApi;
        _logger = logger;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var expiresAt = SessionCookie.AccessTokenExpiresAt(context.Properties);
        if (expiresAt is not null && expiresAt.Value - DateTimeOffset.UtcNow > RefreshMargin)
        {
            return;
        }

        var refreshToken = context.Properties.GetTokenValue(SessionCookie.RefreshTokenName);
        var refreshed = string.IsNullOrEmpty(refreshToken)
            ? null
            : await _authApi.RefreshAsync(refreshToken, context.HttpContext.RequestAborted);

        if (refreshed?.Succeeded != true || refreshed.Value is null)
        {
            _logger.LogInformation("Session for scheme {Scheme} ended (refresh refused).", context.Scheme.Name);
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(context.Scheme.Name);
            return;
        }

        SessionCookie.StoreTokens(context.Properties, refreshed.Value);
        context.ReplacePrincipal(SessionCookie.CreatePrincipal(context.Scheme.Name, refreshed.Value.User));
        context.ShouldRenew = true;
    }
}
