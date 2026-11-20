using FluentResults;
using FluentValidation.Results;

namespace CookieBytes.Core.Results;

internal static class ValidationResultFactory
{
    public static TResponse CreateFailedResponse<TResponse>(IReadOnlyList<ValidationFailure> failures)
    {
        var responseType = typeof(TResponse);
        var errors = failures.Select(f => new Error(f.ErrorMessage)).ToList();

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Fail(errors);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var failMethod = typeof(Result)
                .GetMethods()
                .Single(m => m.Name == nameof(Result.Fail)
                    && m.IsGenericMethodDefinition
                    && m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType == typeof(IEnumerable<IError>));

            var genericFail = failMethod.MakeGenericMethod(valueType);
            return (TResponse)genericFail.Invoke(null, [errors])!;
        }

        throw new InvalidOperationException(
            "CookieBytesCoreValidationMode.FluentResult requires handlers to return FluentResults.Result or Result<T>. " +
            $"Handler returns {responseType.Name}.");
    }
}
