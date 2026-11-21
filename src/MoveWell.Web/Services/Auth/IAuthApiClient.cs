using MoveWell.Common.Contracts.Auth;
using MoveWell.Web.Services.Api;

namespace MoveWell.Web.Services.Auth;

/// <summary>Typed client over <c>/api/v1/auth</c>. Pure transport: no rules here.</summary>
public interface IAuthApiClient
{
    Task<ApiResult> StartRegistrationAsync(string mobile, CancellationToken cancellationToken = default);

    Task<ApiResult<AuthSessionResponse>> CompleteRegistrationAsync(CompleteRegistrationRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<AuthSessionResponse>> SignInAsync(string mobile, string password, CancellationToken cancellationToken = default);

    Task<ApiResult<AuthSessionResponse>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<ApiResult> SignOutAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<ApiResult> StartPasswordResetAsync(string mobile, CancellationToken cancellationToken = default);

    Task<ApiResult> CompletePasswordResetAsync(CompletePasswordResetRequest request, CancellationToken cancellationToken = default);
}
