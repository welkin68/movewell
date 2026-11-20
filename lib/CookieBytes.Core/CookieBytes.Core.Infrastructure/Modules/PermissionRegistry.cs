using System.Collections.Concurrent;
using CookieBytes.Core.Application.Abstractions.Modules;

namespace CookieBytes.Core.Infrastructure.Modules;

/// <summary>
/// Thread-safe permission registry.
/// </summary>
public sealed class PermissionRegistry : IPermissionRegistry
{
    private readonly ConcurrentDictionary<(string ModuleId, string Name), IPermissionDefinition> _permissions = new();

    /// <inheritdoc />
    public void Register(IPermissionDefinition permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        if (!_permissions.TryAdd((permission.ModuleId, permission.Name), permission))
        {
            throw new InvalidOperationException(
                $"Permission '{permission.Name}' is already registered for module '{permission.ModuleId}'.");
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<IPermissionDefinition> GetAll() =>
        _permissions.Values
            .OrderBy(p => p.ModuleId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
}
