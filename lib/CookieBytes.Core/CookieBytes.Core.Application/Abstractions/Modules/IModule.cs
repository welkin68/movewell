using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookieBytes.Core.Application.Abstractions.Modules;

/// <summary>
/// CookieBytes OS kernel module contract.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Gets the stable module identifier (e.g. <c>filemanager</c>).
    /// </summary>
    string ModuleId { get; }

    /// <summary>
    /// Gets the module semver.
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// Gets permissions contributed by this module.
    /// </summary>
    IReadOnlyList<IPermissionDefinition> Permissions { get; }

    /// <summary>
    /// Registers module-specific services.
    /// </summary>
    /// <param name="services">The host service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}
