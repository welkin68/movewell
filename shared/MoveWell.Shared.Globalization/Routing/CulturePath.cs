using Microsoft.AspNetCore.Http;
using MoveWell.Common.Cultures;

namespace MoveWell.Shared.Globalization.Routing;

/// <summary>Builds the same page's URL in the other language (language switch, hreflang alternates).</summary>
public static class CulturePath
{
    public static string CurrentCode(HttpRequest request) =>
        CulturePrefixMiddleware.IsEnglish(request) ? Cultures.En : Cultures.Fa;

    /// <summary>Path + query of the current page for <paramref name="targetCode"/> (fa → no prefix, en → <c>/en</c>).</summary>
    public static string For(HttpRequest request, string targetCode)
    {
        var path = request.Path.HasValue && request.Path.Value != "/" ? request.Path.Value! : string.Empty;
        var query = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;

        return targetCode == Cultures.En
            ? "/" + Cultures.EnRoutePrefix + path + query
            : (path.Length == 0 ? "/" : path) + query;
    }
}
