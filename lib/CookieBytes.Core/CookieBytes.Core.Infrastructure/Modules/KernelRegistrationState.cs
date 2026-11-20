namespace CookieBytes.Core.Infrastructure.Modules;

/// <summary>
/// Holds kernel singleton instances created during service registration.
/// </summary>
internal sealed class KernelRegistrationState
{
    internal KernelRegistrationState()
    {
        ModuleRegistry = new ModuleRegistry();
        PermissionRegistry = new PermissionRegistry();
    }

    internal bool KernelRegistered { get; init; }

    internal ModuleRegistry ModuleRegistry { get; }

    internal PermissionRegistry PermissionRegistry { get; }
}
