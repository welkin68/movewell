using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MoveWell.Common.Contracts.Common;

namespace MoveWell.Web.Services.Api;

/// <summary>
/// Shared plumbing of the typed Api clients: one named HttpClient (<see cref="ClientName"/>), JSON in/out,
/// ProblemDetails <c>errors</c> → <see cref="ApiError"/>, and a localized "service unavailable" error when the Api cannot be reached.
/// </summary>
public abstract class ApiClientBase
{
    public const string ClientName = "MoveWellApi";
    public const string UnavailableCode = "web.api_unavailable";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    protected ApiClientBase(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>Message shown when the Api is down; set by the caller from Web resources (fa/en).</summary>
    protected virtual string UnavailableMessage => "The service is not available right now. Please try again shortly.";

    protected async Task<ApiResult> PostAsync<TBody>(string path, TBody body, CancellationToken cancellationToken)
    {
        var response = await SendAsync(client => client.PostAsJsonAsync(path, body, cancellationToken));
        if (response is null)
        {
            return ApiResult.Fail(HttpStatusCode.ServiceUnavailable, [Unavailable()]);
        }

        using (response)
        {
            return response.IsSuccessStatusCode
                ? ApiResult.Ok(response.StatusCode)
                : ApiResult.Fail(response.StatusCode, await ReadErrorsAsync(response, cancellationToken));
        }
    }

    protected async Task<ApiResult<TResponse>> PostAsync<TBody, TResponse>(string path, TBody body, CancellationToken cancellationToken)
    {
        var response = await SendAsync(client => client.PostAsJsonAsync(path, body, cancellationToken));
        if (response is null)
        {
            return ApiResult<TResponse>.Fail(HttpStatusCode.ServiceUnavailable, [Unavailable()]);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<TResponse>.Fail(response.StatusCode, await ReadErrorsAsync(response, cancellationToken));
            }

            var value = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
            return value is null
                ? ApiResult<TResponse>.Fail(response.StatusCode, [Unavailable()])
                : ApiResult<TResponse>.Ok(response.StatusCode, value);
        }
    }

    private async Task<HttpResponseMessage?> SendAsync(Func<HttpClient, Task<HttpResponseMessage>> send)
    {
        try
        {
            return await send(_httpClientFactory.CreateClient(ClientName));
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(exception, "Api call failed (network/timeout).");
            return null;
        }
    }

    private ApiError Unavailable() => new(UnavailableCode, UnavailableMessage);

    private async Task<IReadOnlyList<ApiError>> ReadErrorsAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            if (document.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array)
            {
                var items = errors.EnumerateArray()
                    .Where(item => item.ValueKind == JsonValueKind.Object)
                    .Select(item => new ApiError(
                        item.TryGetProperty("code", out var code) ? code.GetString() ?? string.Empty : string.Empty,
                        item.TryGetProperty("message", out var message) ? message.GetString() ?? string.Empty : string.Empty))
                    .ToList();
                if (items.Count > 0)
                {
                    return items;
                }
            }
        }
        catch (JsonException)
        {
            // Non-JSON error body (e.g. IIS page) → generic error below.
        }

        _logger.LogWarning("Api answered {Status} without error items.", (int)response.StatusCode);
        return [new ApiError(ApiErrorCodes.Unexpected, UnavailableMessage)];
    }
}
