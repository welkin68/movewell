using FluentValidation;
using MediatR;

namespace CookieBytes.Core.Application.Behaviors;

/// <summary>
/// Runs FluentValidation validators before the handler executes.
/// Validation failures throw <see cref="ValidationException"/>.
/// </summary>
public sealed class ValidationBehaviorThrows<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviorThrows(IEnumerable<IValidator<TRequest>> validators)
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
            throw new ValidationException(failures);
        }

        return await next();
    }
}
