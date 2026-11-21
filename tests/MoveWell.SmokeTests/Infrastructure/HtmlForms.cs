using System.Net;
using System.Text.RegularExpressions;

namespace MoveWell.SmokeTests.Infrastructure;

/// <summary>Minimal "browser" helpers: read a page, take its antiforgery token, post a form like a browser would.</summary>
public static partial class HtmlForms
{
    public static async Task<string> GetPageAsync(this HttpClient browser, string url, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var response = await browser.GetAsync(url);
        Assert.Equal(expected, response.StatusCode);
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>GETs <paramref name="pageUrl"/>, then POSTs <paramref name="fields"/> + its antiforgery token to <paramref name="postUrl"/>.</summary>
    public static async Task<HttpResponseMessage> SubmitAsync(
        this HttpClient browser,
        string pageUrl,
        string postUrl,
        IDictionary<string, string> fields)
    {
        var page = await browser.GetPageAsync(pageUrl);
        var token = AntiforgeryToken().Match(page);
        Assert.True(token.Success, $"No antiforgery token on {pageUrl}.");

        var form = new Dictionary<string, string>(fields) { ["__RequestVerificationToken"] = WebUtility.HtmlDecode(token.Groups[1].Value) };
        return await browser.PostAsync(postUrl, new FormUrlEncodedContent(form));
    }

    public static string? RedirectPath(this HttpResponseMessage response) =>
        response.Headers.Location is { } location
            ? (location.IsAbsoluteUri ? location.PathAndQuery : location.OriginalString)
            : null;

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex AntiforgeryToken();
}
