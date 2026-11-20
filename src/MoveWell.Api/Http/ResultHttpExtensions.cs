using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using MoveWell.Api.Localization;
using MoveWell.Common.Contracts.Common;
using MoveWell.Domain.Shared.Errors;

namespace MoveWell.Api.Http;

/// <summary>
/// The single place that turns a <see cref="Result"/> into HTTP (backend-guide §5.3).
/// Failures become ProblemDetails with an <c>errors</c> array of { code, message } — message localized by Accept-Language.
/// </summary>
public static class ResultHttpExtensions
{
    public const string ErrorsExtension = "errors";

    public static IResult ToHttp(this Result result, HttpContext context) =>
        result.IsSuccess ? TypedResults.NoContent() : Problem(result.Errors, context);

    public static IResult ToHttp<TValue, TResponse>(this Result<TValue> result, HttpContext context, Func<TValue, TResponse> map) =>
        result.IsSuccess ? TypedResults.Ok(map(result.Value)) : Problem(result.Errors, context);

    public static ProblemHttpResult Problem(IReadOnlyList<IError> errors, HttpContext context)
    {
        var messages = context.RequestServices.GetRequiredService<ErrorMessages>();
        var culture = context.GetCulture();

        var items = errors.Select(error => ToApiError(error, messages, culture)).ToList();
        var status = StatusFor(errors);

        if (status == StatusCodes.Status429TooManyRequests
            && errors.OfType<DomainError>().FirstOrDefault(e => e.Metadata.ContainsKey("seconds")) is { } wait)
        {
            context.Response.Headers.RetryAfter = Convert.ToString(wait.Metadata["seconds"], System.Globalization.CultureInfo.InvariantCulture);
        }

        return TypedResults.Problem(
            title: items[0].Message,
            statusCode: status,
            extensions: new Dictionary<string, object?> { [ErrorsExtension] = items });
    }

    private static ApiError ToApiError(IError error, ErrorMessages messages, Domain.Shared.Enumerations.Culture culture)
    {
        // DomainError carries its code; plain errors come from FluentValidation, whose message IS the code.
        var code = error is DomainError domain ? domain.Code : error.Message;
        return new ApiError(code, messages.Get(code, culture, error.Message, error.Metadata));
    }

    private static int StatusFor(IReadOnlyList<IError> errors)
    {
        var kind = errors.OfType<DomainError>().Select(error => error.Kind).FirstOrDefault() ?? ErrorKind.Validation;
        return kind.Name switch
        {
            nameof(ErrorKind.NotFound) => StatusCodes.Status404NotFound,
            nameof(ErrorKind.Conflict) => StatusCodes.Status409Conflict,
            nameof(ErrorKind.Forbidden) => StatusCodes.Status403Forbidden,
            nameof(ErrorKind.Unauthorized) => StatusCodes.Status401Unauthorized,
            nameof(ErrorKind.TooManyRequests) => StatusCodes.Status429TooManyRequests,
            _ => StatusCodes.Status400BadRequest,
        };
    }
}
