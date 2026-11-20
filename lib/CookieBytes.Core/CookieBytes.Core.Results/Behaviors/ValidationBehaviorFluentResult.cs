using CookieBytes.Core.Application.Behaviors;
using FluentValidation;
using MediatR;

namespace CookieBytes.Core.Results.Behaviors;

/// <summary>
/// Runs FluentValidation validators before the handler executes.
/// Validation failures return failed FluentResults <c>Result</c> or <c>Result&lt;T&gt;</c>.
/// </summary>
public sealed class ValidationBehaviorFluentResult<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviorFluentResult(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var failures = await ValidationPipelineRunner.ValidateAsync(_validators, request, cancellationToken);
        if (failures is not null)
        {
            return ValidationResultFactory.CreateFailedResponse<TResponse>(failures);
        }

        return await next();
    }
}
