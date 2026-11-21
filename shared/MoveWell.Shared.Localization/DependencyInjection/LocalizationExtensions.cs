using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using MoveWell.Shared.Localization.Json;

namespace MoveWell.Shared.Localization.DependencyInjection;

public static class LocalizationExtensions
{
    /// <summary>
    /// Replaces the resx factory with the JSON one. Call AFTER anything that calls AddLocalization
    /// (e.g. AddDataAnnotationsLocalization), otherwise the resx factory wins and pages show keys.
    /// </summary>
    public static IServiceCollection AddMoveWellJsonLocalization(this IServiceCollection services, Action<JsonLocalizationOptions>? configure = null)
    {
        services.Configure(configure ?? (_ => { }));
        services.RemoveAll<IStringLocalizerFactory>();
        services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
        services.TryAddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
        return services;
    }
}
