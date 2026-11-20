namespace CookieBytes.Core.Application.Abstractions.Modules;

/// <summary>
/// A permission contributed by a module.
/// </summary>
public interface IPermissionDefinition
{
    /// <summary>
    /// Gets the owning module identifier.
    /// </summary>
    string ModuleId { get; }

    /// <summary>
    /// Gets the permission name (unique within the module).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets an optional human-readable description.
    /// </summary>
    string? Description { get; }
}
