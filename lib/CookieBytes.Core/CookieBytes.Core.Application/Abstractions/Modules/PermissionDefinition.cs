namespace CookieBytes.Core.Application.Abstractions.Modules;

/// <summary>
/// Default permission definition implementation.
/// </summary>
/// <param name="ModuleId">The owning module identifier.</param>
/// <param name="Name">The permission name.</param>
/// <param name="Description">An optional description.</param>
public sealed record PermissionDefinition(string ModuleId, string Name, string? Description = null)
    : IPermissionDefinition;
