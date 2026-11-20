using FluentValidation;
using FluentValidation.Results;

namespace CookieBytes.Core.Application.Behaviors;

internal static class ValidationPipelineRunner
{
    public static async Task<IReadOnlyList<ValidationFailure>?> ValidateAsync<TRequest>(
        IEnumerable<IValidator<TRequest>> validators,
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        var validatorList = validators as IReadOnlyList<IValidator<TRequest>> ?? validators.ToList();
        if (validatorList.Count == 0)
        {
            return null;
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validatorList.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        return failures.Count == 0 ? null : failures;
    }
}
