namespace CookieBytes.Core.Application.Abstractions.Modules;

/// <summary>
/// Tracks modules registered with the host kernel.
/// </summary>
public interface IModuleRegistry
{
    /// <summary>
    /// Registers a module with the kernel.
    /// </summary>
    /// <param name="module">The module instance.</param>
    void Register(IModule module);

    /// <summary>
    /// Gets all registered modules.
    /// </summary>
    IReadOnlyList<ModuleDescriptor> GetModules();
}
