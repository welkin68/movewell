using System.Collections.Frozen;
using System.Text.Json;
using CookieBytes.Core.Domain.SeedWork;
using MoveWell.Domain.Shared.Enumerations;

namespace MoveWell.Api.Localization;

/// <summary>
/// Error code → localized message (fa/en) from <c>Localization/Errors/errors.{fa|en}.json</c>.
/// Placeholders like <c>{seconds}</c> are filled from the error's metadata. Unknown codes fall back to the English text supplied.
/// </summary>
public sealed class ErrorMessages
{
    private readonly FrozenDictionary<string, FrozenDictionary<string, string>> _byCulture;

    public ErrorMessages(IWebHostEnvironment environment)
    {
        var folder = Path.Combine(environment.ContentRootPath, "Localization", "Errors");
        _byCulture = Enumeration.GetAll<Culture>().ToFrozenDictionary(
            culture => culture.Name!,
            culture => Load(Path.Combine(folder, $"errors.{culture.Name}.json")));
    }

    public string Get(string code, Culture culture, string fallback, IReadOnlyDictionary<string, object>? arguments = null)
    {
        var text = _byCulture.TryGetValue(culture.Name!, out var messages) && messages.TryGetValue(code, out var found)
            ? found
            : fallback;

        if (arguments is null)
        {
            return text;
        }

        foreach (var (name, value) in arguments)
        {
            text = text.Replace("{" + name + "}", Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
        }

        return text;
    }

    public bool Contains(string code, Culture culture) =>
        _byCulture.TryGetValue(culture.Name!, out var messages) && messages.ContainsKey(code);

    private static FrozenDictionary<string, string> Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Error message file is missing.", path);
        }

        var values = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))
            ?? throw new InvalidOperationException($"{path} is empty.");
        return values.ToFrozenDictionary(StringComparer.Ordinal);
    }
}
