using System.Net;
using MoveWell.Common.Contracts.Common;

namespace MoveWell.Web.Services.Api;

/// <summary>Outcome of one Api call. Errors are the Api's { code, message } items (message already in the page language).</summary>
public class ApiResult
{
    protected ApiResult(HttpStatusCode status, IReadOnlyList<ApiError> errors)
    {
        Status = status;
        Errors = errors;
    }

    public HttpStatusCode Status { get; }

    public IReadOnlyList<ApiError> Errors { get; }

    public bool Succeeded => Errors.Count == 0 && (int)Status is >= 200 and < 300;

    public static ApiResult Ok(HttpStatusCode status) => new(status, []);

    public static ApiResult Fail(HttpStatusCode status, IReadOnlyList<ApiError> errors) => new(status, errors);
}

public sealed class ApiResult<T> : ApiResult
{
    private ApiResult(HttpStatusCode status, T? value, IReadOnlyList<ApiError> errors)
        : base(status, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static ApiResult<T> Ok(HttpStatusCode status, T value) => new(status, value, []);

    public static new ApiResult<T> Fail(HttpStatusCode status, IReadOnlyList<ApiError> errors) => new(status, default, errors);
}
