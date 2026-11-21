using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using MoveWell.Common.Cultures;

namespace MoveWell.Shared.Globalization.Routing;

/// <summary>Request culture from the URL: PathBase <c>/en</c> → en-US, anything else → fa-IR.</summary>
public sealed class PathBaseRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var name = CulturePrefixMiddleware.IsEnglish(httpContext.Request) ? Cultures.EnCultureName : Cultures.FaCultureName;
        return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(name, name));
    }
}
