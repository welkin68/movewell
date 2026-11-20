using Microsoft.Extensions.Configuration;

namespace CookieBytes.Core.Application.Abstractions.Modules;

/// <summary>
/// Typed access to <c>CookieBytes:*</c> configuration sections.
/// </summary>
public interface ICookieBytesConfiguration
{
    /// <summary>
    /// Gets the <c>CookieBytes:Core</c> configuration section.
    /// </summary>
    IConfigurationSection CoreSection { get; }

    /// <summary>
    /// Gets a section relative to <c>CookieBytes</c> (e.g. <c>Core</c> or <c>Modules</c>).
    /// </summary>
    /// <param name="relativePath">Path relative to <c>CookieBytes</c>.</param>
    IConfigurationSection GetSection(string relativePath);

    /// <summary>
    /// Binds a section relative to <c>CookieBytes</c> to a new instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The options type.</typeparam>
    /// <param name="relativePath">Path relative to <c>CookieBytes</c>.</param>
    T Bind<T>(string relativePath)
        where T : class, new();
}
