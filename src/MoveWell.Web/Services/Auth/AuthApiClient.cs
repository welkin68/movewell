using Microsoft.Extensions.Localization;
using MoveWell.Common.Contracts.Auth;
using MoveWell.Web.Services.Api;

namespace MoveWell.Web.Services.Auth;

public sealed class AuthApiClient : ApiClientBase, IAuthApiClient
{
    private const string Root = "api/v1/auth/";
    private readonly IStringLocalizer _shared;

    public AuthApiClient(IHttpClientFactory httpClientFactory, ILogger<AuthApiClient> logger, IStringLocalizerFactory localizers)
        : base(httpClientFactory, logger)
    {
        _shared = localizers.Create("public/shared/common", string.Empty);
    }

    protected override string UnavailableMessage => _shared["error.api_unavailable"];

    public Task<ApiResult> StartRegistrationAsync(string mobile, CancellationToken cancellationToken = default) =>
        PostAsync(Root + "register/start", new StartRegistrationRequest(mobile), cancellationToken);

    public Task<ApiResult<AuthSessionResponse>> CompleteRegistrationAsync(CompleteRegistrationRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<CompleteRegistrationRequest, AuthSessionResponse>(Root + "register/complete", request, cancellationToken);

    public Task<ApiResult<AuthSessionResponse>> SignInAsync(string mobile, string password, CancellationToken cancellationToken = default) =>
        PostAsync<SignInRequest, AuthSessionResponse>(Root + "signin", new SignInRequest(mobile, password), cancellationToken);

    public Task<ApiResult<AuthSessionResponse>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default) =>
        PostAsync<RefreshSessionRequest, AuthSessionResponse>(Root + "refresh", new RefreshSessionRequest(refreshToken), cancellationToken);

    public Task<ApiResult> SignOutAsync(string refreshToken, CancellationToken cancellationToken = default) =>
        PostAsync(Root + "signout", new SignOutRequest(refreshToken), cancellationToken);

    public Task<ApiResult> StartPasswordResetAsync(string mobile, CancellationToken cancellationToken = default) =>
        PostAsync(Root + "password/reset/start", new StartPasswordResetRequest(mobile), cancellationToken);

    public Task<ApiResult> CompletePasswordResetAsync(CompletePasswordResetRequest request, CancellationToken cancellationToken = default) =>
        PostAsync(Root + "password/reset/complete", request, cancellationToken);
}
