using MediatR;
using MoveWell.Api.Http;
using MoveWell.Api.RateLimiting;
using MoveWell.Application.DTOs.Auth;
using MoveWell.Application.Features.Auth.Commands.ChangePassword;
using MoveWell.Application.Features.Auth.Commands.CompletePasswordReset;
using MoveWell.Application.Features.Auth.Commands.CompleteRegistration;
using MoveWell.Application.Features.Auth.Commands.RefreshSession;
using MoveWell.Application.Features.Auth.Commands.SignIn;
using MoveWell.Application.Features.Auth.Commands.SignOut;
using MoveWell.Application.Features.Auth.Commands.StartPasswordReset;
using MoveWell.Application.Features.Auth.Commands.StartRegistration;
using MoveWell.Common.Contracts.Auth;
using MoveWell.Common.Identity;

namespace MoveWell.Api.Endpoints.Auth;

/// <summary>/api/v1/auth — thin: request → command → Result → HTTP. No logic here.</summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/register/start", async (StartRegistrationRequest body, ISender sender, HttpContext context, CancellationToken ct) =>
                (await sender.Send(new StartRegistrationCommand(body.Mobile, context.GetCulture().Name!), ct)).ToHttp(context))
            .RequireRateLimiting(RateLimitPolicies.OtpSend);

        group.MapPost("/register/complete", async (CompleteRegistrationRequest body, ISender sender, HttpContext context, CancellationToken ct) =>
                (await sender.Send(
                    new CompleteRegistrationCommand(body.Mobile, body.Code, body.Password, body.DisplayName, context.GetCulture().Name!, context.GetClientIp()),
                    ct)).ToHttp(context, ToResponse))
            .RequireRateLimiting(RateLimitPolicies.Auth);

        group.MapPost("/signin", async (SignInRequest body, ISender sender, HttpContext context, CancellationToken ct) =>
                (await sender.Send(new SignInCommand(body.Mobile, body.Password, context.GetClientIp()), ct)).ToHttp(context, ToResponse))
            .RequireRateLimiting(RateLimitPolicies.Auth);

        group.MapPost("/refresh", async (RefreshSessionRequest body, ISender sender, HttpContext context, CancellationToken ct) =>
                (await sender.Send(new RefreshSessionCommand(body.RefreshToken, context.GetClientIp()), ct)).ToHttp(context, ToResponse))
            .RequireRateLimiting(RateLimitPolicies.Auth);

        group.MapPost("/signout", async (SignOutRequest body, ISender sender, HttpContext context, CancellationToken ct) =>
            (await sender.Send(new SignOutCommand(body.RefreshToken), ct)).ToHttp(context));

        group.MapPost("/password/reset/start", async (StartPasswordResetRequest body, ISender sender, HttpContext context, CancellationToken ct) =>
                (await sender.Send(new StartPasswordResetCommand(body.Mobile, context.GetCulture().Name!), ct)).ToHttp(context))
            .RequireRateLimiting(RateLimitPolicies.OtpSend);

        group.MapPost("/password/reset/complete", async (CompletePasswordResetRequest body, ISender sender, HttpContext context, CancellationToken ct) =>
                (await sender.Send(new CompletePasswordResetCommand(body.Mobile, body.Code, body.NewPassword), ct)).ToHttp(context))
            .RequireRateLimiting(RateLimitPolicies.Auth);

        group.MapPost("/password/change", async (ChangePasswordRequest body, ISender sender, HttpContext context, CancellationToken ct) =>
                (await sender.Send(new ChangePasswordCommand(body.CurrentPassword, body.NewPassword), ct)).ToHttp(context))
            .RequireAuthorization(Policies.Member)
            .RequireRateLimiting(RateLimitPolicies.Auth);

        return endpoints;
    }

    private static AuthSessionResponse ToResponse(AuthSessionDto session) => new(
        session.AccessToken,
        session.AccessTokenExpiresAt,
        session.RefreshToken,
        session.RefreshTokenExpiresAt,
        new AuthUserResponse(session.User.Id, session.User.Mobile, session.User.DisplayName, session.User.Culture, session.User.Roles));
}
