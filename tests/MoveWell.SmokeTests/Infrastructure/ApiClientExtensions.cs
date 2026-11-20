using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MoveWell.Common.Contracts.Common;

namespace MoveWell.SmokeTests.Infrastructure;

public static class ApiClientExtensions
{
    private static int _mobileCounter = Random.Shared.Next(1_000_000, 4_000_000);

    /// <summary>A fresh, valid mobile number per test so tests never share OTP limits or accounts.</summary>
    public static string NewMobile() => "0912" + Interlocked.Increment(ref _mobileCounter).ToString("D7");

    public static HttpClient WithLanguage(this HttpClient client, string language)
    {
        client.DefaultRequestHeaders.AcceptLanguage.Clear();
        client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(language));
        return client;
    }

    public static HttpClient WithBearer(this HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public static async Task<IReadOnlyList<ApiError>> ReadErrorsAsync(this HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("errors")
            .EnumerateArray()
            .Select(item => new ApiError(item.GetProperty("code").GetString()!, item.GetProperty("message").GetString()!))
            .ToList();
    }

    public static Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, string url, T body) =>
        client.PostAsJsonAsync(url, body);
}
