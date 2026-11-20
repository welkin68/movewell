using CookieBytes.Core.Infrastructure.DependencyInjection;
using CookieBytes.Core.Results.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CookieBytes.Core.Results.DependencyInjection;

/// <summary>
/// FluentResults integration for CookieBytes.Core.
/// </summary>
public static class ResultsServiceCollectionExtensions
{
    /// <summary>
    /// Registers FluentResult validation pipeline behavior.
    /// Required when <see cref="CookieBytesCoreOptions.ValidationMode"/> is <see cref="CookieBytesCoreValidationMode.FluentResult"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCookieBytesCoreResults(this IServiceCollection services)
    {
        services.Configure<CookieBytesCoreOptions>(options => options.FluentResultsRegistered = true);

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviorFluentResult<,>)));

        return services;
    }
}
