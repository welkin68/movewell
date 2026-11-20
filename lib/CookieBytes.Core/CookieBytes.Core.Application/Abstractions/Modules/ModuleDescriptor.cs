namespace CookieBytes.Core.Application.Abstractions.Modules;

/// <summary>
/// Immutable snapshot of a registered module.
/// </summary>
/// <param name="ModuleId">The module identifier.</param>
/// <param name="Version">The module version.</param>
/// <param name="AssemblyName">The assembly containing the module type.</param>
public sealed record ModuleDescriptor(string ModuleId, Version Version, string AssemblyName);
