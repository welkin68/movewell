namespace CookieBytes.Core.Application.Abstractions.Modules;

/// <summary>
/// Aggregates permissions from all registered modules.
/// </summary>
public interface IPermissionRegistry
{
    /// <summary>
    /// Registers a permission definition.
    /// </summary>
    /// <param name="permission">The permission to register.</param>
    void Register(IPermissionDefinition permission);

    /// <summary>
    /// Gets all registered permissions.
    /// </summary>
    IReadOnlyList<IPermissionDefinition> GetAll();
}
