using System.Collections.Concurrent;
using CookieBytes.Core.Application.Abstractions.Modules;

namespace CookieBytes.Core.Infrastructure.Modules;

/// <summary>
/// Thread-safe module registry.
/// </summary>
public sealed class ModuleRegistry : IModuleRegistry
{
    private readonly ConcurrentDictionary<string, ModuleDescriptor> _modules = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void Register(IModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        var descriptor = new ModuleDescriptor(
            module.ModuleId,
            module.Version,
            module.GetType().Assembly.GetName().Name ?? module.GetType().Assembly.FullName ?? module.GetType().Name);

        if (!_modules.TryAdd(module.ModuleId, descriptor))
        {
            throw new InvalidOperationException($"Module '{module.ModuleId}' is already registered.");
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<ModuleDescriptor> GetModules() =>
        _modules.Values.OrderBy(m => m.ModuleId, StringComparer.OrdinalIgnoreCase).ToList();
}
