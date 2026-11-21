using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MoveWell.Web.Localization;
using MoveWell.Web.Services.Api;

namespace MoveWell.SmokeTests.Infrastructure;

/// <summary>
/// The whole site in memory: the real Web (BFF) whose Api HttpClient is wired straight into the in-memory Api.
/// Browser → Web → Api → SQL Server, exactly like production, minus the network.
/// </summary>
public sealed class SiteFixture : IAsyncLifetime
{
    public ApiFactory Api { get; } = new();

    public WebFactory Web { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await Api.InitializeAsync();
        Web = new WebFactory(Api);
    }

    public async Task DisposeAsync()
    {
        await Web.DisposeAsync();
        await ((IAsyncLifetime)Api).DisposeAsync();
    }

    /// <summary>A browser-like client: keeps cookies, does not follow redirects (tests assert them).</summary>
    public HttpClient Browser() =>
        Web.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
}

public sealed class WebFactory : WebApplicationFactory<UiText>
{
    private readonly ApiFactory _api;

    public WebFactory(ApiFactory api)
    {
        _api = api;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Api:BaseUrl", "http://localhost/");
        builder.UseSetting("DataProtection:KeysPath", Path.Combine(Path.GetTempPath(), "movewell-smoke-keys"));
        builder.UseSetting("Logging:Directory", Path.Combine(Path.GetTempPath(), "movewell-smoke-logs"));

        builder.ConfigureTestServices(services =>
            services.AddHttpClient(ApiClientBase.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => _api.Server.CreateHandler()));
    }
}
