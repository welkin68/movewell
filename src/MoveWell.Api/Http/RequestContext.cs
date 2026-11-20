using MoveWell.Domain.Shared.Enumerations;

namespace MoveWell.Api.Http;

public static class RequestContext
{
    /// <summary>First language of <c>Accept-Language</c> (fa default). Web sends the page culture; the mobile app sends its UI language.</summary>
    public static Culture GetCulture(this HttpContext context)
    {
        var header = context.Request.Headers.AcceptLanguage.ToString();
        var first = header.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
        return Culture.FromCode(first?.Split(';')[0]);
    }

    /// <summary>Client IP after forwarded-headers processing (Web forwards the browser's IP; only loopback proxies are trusted).</summary>
    public static string? GetClientIp(this HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString();
}
