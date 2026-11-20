namespace MoveWell.Common.Contracts.Common;

/// <summary>
/// One item of the <c>errors</c> array inside every error response (RFC 7807 ProblemDetails).
/// <see cref="Code"/> is stable (clients may branch on it); <see cref="Message"/> is already localized (fa/en).
/// </summary>
public sealed record ApiError(string Code, string Message);

public static class ApiErrorCodes
{
    public const string Unexpected = "general.unexpected";
    public const string RateLimited = "general.rate_limited";
    public const string Unauthorized = "general.unauthorized";
    public const string Forbidden = "general.forbidden";
}
