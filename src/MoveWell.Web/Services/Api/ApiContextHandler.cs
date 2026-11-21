using System.Globalization;
using MoveWell.Web.Middleware;

namespace MoveWell.Web.Services.Api;

/// <summary>
/// Adds the request context to every Api call: <c>Accept-Language</c> (page culture, so Api errors come back in the same language),
/// <c>X-Correlation-Id</c> (trace one action across Web and Api logs) and <c>X-Forwarded-For</c> (browser IP for Api rate limits).
/// </summary>
public sealed class ApiContextHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiContextHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.AcceptLanguage.Clear();
        request.Headers.AcceptLanguage.ParseAdd(CultureInfo.CurrentUICulture.Name);

        var context = _httpContextAccessor.HttpContext;
        if (context is not null)
        {
            request.Headers.Remove(CorrelationIdMiddleware.HeaderName);
            request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, context.TraceIdentifier);

            var ip = context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ip))
            {
                request.Headers.Remove("X-Forwarded-For");
                request.Headers.TryAddWithoutValidation("X-Forwarded-For", ip);
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}
