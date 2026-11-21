using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using MoveWell.Common.Cultures;
using MoveWell.Shared.Globalization.Routing;

namespace MoveWell.Shared.Globalization.DependencyInjection;

public static class GlobalizationExtensions
{
    public static IServiceCollection AddMoveWellRequestLocalization(this IServiceCollection services)
    {
        services.AddRequestLocalization(options =>
        {
            var fa = new CultureInfo(Cultures.FaCultureName);
            var en = new CultureInfo(Cultures.EnCultureName);
            options.DefaultRequestCulture = new RequestCulture(fa);
            options.SupportedCultures = [fa, en];
            options.SupportedUICultures = [fa, en];
            options.ApplyCurrentCultureToResponseHeaders = true;
            options.RequestCultureProviders = [new PathBaseRequestCultureProvider()];
        });

        return services;
    }

    /// <summary>Culture prefix first (it rewrites PathBase), then request localization. Call before UseRouting.</summary>
    public static IApplicationBuilder UseMoveWellCultures(this IApplicationBuilder app) =>
        app.UseMiddleware<CulturePrefixMiddleware>().UseRequestLocalization();
}
