using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace MoveWell.Shared.Localization.Json;

/// <summary>
/// Reads <c>{ResourcesPath}/{baseName}.{culture}.json</c> (e.g. <c>Resources/public/account/signin.fa.json</c>),
/// trying the specific culture (fa-IR) then its parent (fa). Missing keys render as the key, so gaps are visible.
/// </summary>
public sealed class JsonStringLocalizer : IStringLocalizer
{
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _cache;
    private readonly string _resourcesPath;
    private readonly string _baseName;

    public JsonStringLocalizer(ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> cache, string resourcesPath, string baseName)
    {
        _cache = cache;
        _resourcesPath = resourcesPath;
        _baseName = baseName.Replace('\\', '/').Trim('/');
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = Find(name);
            return new LocalizedString(name, value ?? name, resourceNotFound: value is null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var format = Find(name);
            var value = format is null ? name : string.Format(CultureInfo.CurrentUICulture, format, arguments);
            return new LocalizedString(name, value, resourceNotFound: format is null);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var culture = CultureInfo.CurrentUICulture;
        foreach (var pair in Load(culture.Name))
        {
            yield return new LocalizedString(pair.Key, pair.Value, resourceNotFound: false);
        }

        if (includeParentCultures && !string.IsNullOrEmpty(culture.Parent.Name))
        {
            foreach (var pair in Load(culture.Parent.Name))
            {
                yield return new LocalizedString(pair.Key, pair.Value, resourceNotFound: false);
            }
        }
    }

    private string? Find(string name)
    {
        var culture = CultureInfo.CurrentUICulture;
        if (Load(culture.Name).TryGetValue(name, out var value))
        {
            return value;
        }

        return !string.IsNullOrEmpty(culture.Parent.Name) && Load(culture.Parent.Name).TryGetValue(name, out value)
            ? value
            : null;
    }

    private IReadOnlyDictionary<string, string> Load(string cultureName) =>
        _cache.GetOrAdd($"{_baseName}|{cultureName}", _ =>
        {
            var path = Path.Combine(_resourcesPath, $"{_baseName}.{cultureName}.json".Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            using var stream = File.OpenRead(path);
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(stream) ?? [];
            return new Dictionary<string, string>(data, StringComparer.OrdinalIgnoreCase);
        });
}
