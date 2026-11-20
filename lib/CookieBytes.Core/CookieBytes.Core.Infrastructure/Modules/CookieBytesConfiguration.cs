using CookieBytes.Core.Application.Abstractions.Modules;
using Microsoft.Extensions.Configuration;

namespace CookieBytes.Core.Infrastructure.Modules;

/// <summary>
/// Binds <c>CookieBytes:*</c> configuration sections.
/// </summary>
public sealed class CookieBytesConfiguration : ICookieBytesConfiguration
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes configuration access from the application root configuration.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    public CookieBytesConfiguration(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc />
    public IConfigurationSection CoreSection => GetSection("Core");

    /// <inheritdoc />
    public IConfigurationSection GetSection(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        return _configuration.GetSection($"CookieBytes:{relativePath}");
    }

    /// <inheritdoc />
    public T Bind<T>(string relativePath)
        where T : class, new()
    {
        var instance = new T();
        GetSection(relativePath).Bind(instance);
        return instance;
    }
}
