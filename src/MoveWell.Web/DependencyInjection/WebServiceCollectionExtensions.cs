using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoveWell.Common.Identity;
using MoveWell.Web.Authentication;
using MoveWell.Web.Services.Api;
using MoveWell.Web.Services.Auth;
using Serilog;
using Serilog.Events;

namespace MoveWell.Web.DependencyInjection;

public static class WebServiceCollectionExtensions
{
    public const string StaffPolicy = "Staff";

    public static WebApplicationBuilder AddMoveWellWebSerilog(this WebApplicationBuilder builder)
    {
        var directory = builder.Configuration["Logging:Directory"] ?? "logs";
        builder.Services.AddSerilog((services, configuration) => configuration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "MoveWell.Web")
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(directory, "movewell-web-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {SourceContext} {Message:lj}{NewLine}{Exception}"));
        return builder;
    }

    /// <summary>The single named HttpClient to the Api + typed clients on top of it.</summary>
    public static IServiceCollection AddMoveWellApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ApiOptions>().Bind(configuration.GetSection(ApiOptions.SectionName));
        services.AddHttpContextAccessor();
        services.AddTransient<ApiContextHandler>();

        services.AddHttpClient(ApiClientBase.ClientName, (provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<ApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/");
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddHttpMessageHandler<ApiContextHandler>();

        services.AddScoped<IAuthApiClient, AuthApiClient>();
        return services;
    }

    /// <summary>BFF cookies (D-16) + persisted Data Protection keys (otherwise an IIS recycle signs everyone out).</summary>
    public static IServiceCollection AddMoveWellWebAuth(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var keysPath = configuration["DataProtection:KeysPath"] ?? Path.Combine(environment.ContentRootPath, "App_Data", "keys");
        services.AddDataProtection()
            .SetApplicationName("MoveWell.Web")
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

        services.AddScoped<SessionRefresher>();

        services.AddAuthentication(WebAuthSchemes.Member)
            .AddCookie(WebAuthSchemes.Member, options => ConfigureCookie(options, ".movewell.member", "/account/signin", environment))
            .AddCookie(WebAuthSchemes.Staff, options => ConfigureCookie(options, ".movewell.staff", "/admin/account/signin", environment));

        services.AddAuthorizationBuilder()
            .AddPolicy(StaffPolicy, policy => policy
                .AddAuthenticationSchemes(WebAuthSchemes.Staff)
                .RequireRole(Roles.Admin, Roles.Content));

        return services;
    }

    public static IMvcBuilder AddMoveWellMvc(this IServiceCollection services) =>
        services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

    private static void ConfigureCookie(CookieAuthenticationOptions options, string cookieName, string loginPath, IWebHostEnvironment environment)
    {
        options.Cookie.Name = cookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = environment.IsDevelopment() || environment.IsEnvironment("Testing")
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.LoginPath = loginPath;
        options.AccessDeniedPath = loginPath;
        options.SlidingExpiration = false;
        options.EventsType = typeof(SessionRefresher);
    }
}
