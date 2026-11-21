using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace MoveWell.Shared.Localization.Json;

public sealed class JsonLocalizationOptions
{
    /// <summary>Relative to the content root. Default <c>Resources</c>.</summary>
    public string ResourcesPath { get; set; } = "Resources";
}

/// <summary>
/// <c>Create("public/account/signin", "")</c> → JSON files under <c>Resources/public/account/</c>.
/// <c>Create(typeof(X))</c> maps <c>MoveWell.Web.Resources.Public.Home.Index</c> → <c>public/home/index</c>.
/// </summary>
public sealed class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private const string Marker = ".Resources.";

    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _resourcesPath;

    public JsonStringLocalizerFactory(IWebHostEnvironment environment, IOptions<JsonLocalizationOptions> options)
    {
        var relative = string.IsNullOrWhiteSpace(options.Value.ResourcesPath) ? "Resources" : options.Value.ResourcesPath;
        _resourcesPath = Path.Combine(environment.ContentRootPath, relative);
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        ArgumentNullException.ThrowIfNull(resourceSource);
        var name = resourceSource.FullName ?? resourceSource.Name;
        var index = name.LastIndexOf(Marker, StringComparison.Ordinal);
        if (index >= 0)
        {
            name = name[(index + Marker.Length)..];
        }

        return Create(name.Replace('.', '/').ToLowerInvariant(), string.Empty);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseName);
        return new JsonStringLocalizer(_cache, _resourcesPath, baseName);
    }
}
