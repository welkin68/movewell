using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CookieBytes.Core.Infrastructure.DependencyInjection;

internal sealed class CookieBytesCoreOutboxOptionsValidator : IValidateOptions<CookieBytesCoreOptions>
{
    private readonly IHostEnvironment? _environment;

    public CookieBytesCoreOutboxOptionsValidator(IServiceProvider serviceProvider)
    {
        _environment = serviceProvider.GetService(typeof(IHostEnvironment)) as IHostEnvironment;
    }

    public ValidateOptionsResult Validate(string? name, CookieBytesCoreOptions options)
    {
        if (!options.ValidateOutboxProcessorOnStart)
        {
            return ValidateOptionsResult.Success;
        }

        if (_environment is null || _environment.IsDevelopment())
        {
            return ValidateOptionsResult.Success;
        }

        if (options.RegisterNoOpOutboxProcessor)
        {
            return ValidateOptionsResult.Fail(
                "RegisterNoOpOutboxProcessor cannot be enabled outside Development. " +
                "Register a production IOutboxProcessor via AddCookieBytesCoreOutboxProcessor<T>(). See docs/OUTBOX.md.");
        }

        return ValidateOptionsResult.Success;
    }
}
