using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CookieBytes.Core.Infrastructure.DependencyInjection;

internal sealed class CookieBytesCoreAuditOptionsValidator : IValidateOptions<CookieBytesCoreOptions>
{
    private readonly IHostEnvironment? _environment;

    public CookieBytesCoreAuditOptionsValidator(IServiceProvider serviceProvider)
    {
        _environment = serviceProvider.GetService(typeof(IHostEnvironment)) as IHostEnvironment;
    }

    public ValidateOptionsResult Validate(string? name, CookieBytesCoreOptions options)
    {
        if (!options.ValidateAuditServicesOnStart)
        {
            return ValidateOptionsResult.Success;
        }

        if (_environment is null || _environment.IsDevelopment())
        {
            return ValidateOptionsResult.Success;
        }

        if (options.RegisterDefaultAuditServices)
        {
            return ValidateOptionsResult.Fail(
                "RegisterDefaultAuditServices cannot be enabled outside Development. " +
                "Register ICurrentUserService and IDateTimeProvider for production. See docs/AUDIT.md.");
        }

        return ValidateOptionsResult.Success;
    }
}
