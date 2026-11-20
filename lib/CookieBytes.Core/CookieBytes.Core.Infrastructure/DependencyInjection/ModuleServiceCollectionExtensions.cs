using CookieBytes.Core.Application.Abstractions.Modules;
using CookieBytes.Core.Infrastructure.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CookieBytes.Core.Infrastructure.DependencyInjection;

/// <summary>
/// CookieBytes OS kernel module registration.
/// </summary>
public static class ModuleServiceCollectionExtensions
{
    /// <summary>
    /// Registers a CookieBytes kernel module and its permissions.
    /// Requires a prior call to <see cref="ServiceCollectionExtensions.AddCookieBytesCore"/>.
    /// </summary>
    /// <typeparam name="TModule">The module type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCookieBytesModule<TModule>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TModule : class, IModule, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var kernelState = GetKernelRegistrationState(services)
            ?? throw new InvalidOperationException(
                "AddCookieBytesModule requires AddCookieBytesCore to be called first.");

        services.TryAddSingleton<ICookieBytesConfiguration>(_ => new CookieBytesConfiguration(configuration));

        var module = new TModule();
        kernelState.ModuleRegistry.Register(module);

        foreach (var permission in module.Permissions)
        {
            kernelState.PermissionRegistry.Register(permission);
        }

        module.ConfigureServices(services, configuration);
        return services;
    }

    internal static KernelRegistrationState? GetKernelRegistrationState(IServiceCollection services)
    {
        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType == typeof(KernelRegistrationState)
                && descriptor.ImplementationInstance is KernelRegistrationState state)
            {
                return state.KernelRegistered ? state : null;
            }
        }

        return null;
    }
}
