using Microsoft.AspNetCore.HttpOverrides;
using MoveWell.Api.Endpoints.Auth;
using MoveWell.Api.Endpoints.Health;
using MoveWell.Api.Localization;
using MoveWell.Api.Middleware;
using MoveWell.Api.RateLimiting;
using MoveWell.Infrastructure.DependencyInjection;
using MoveWell.Infrastructure.Host;
using MoveWell.Infrastructure.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
builder.AddMoveWellSerilog("MoveWell.Api");

builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ErrorMessages>();
builder.Services.AddMoveWellInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddMoveWellJwtAuthentication();
builder.Services.AddMoveWellRateLimiting(builder.Configuration);

// Web (the BFF) calls the Api from the same server and forwards the browser IP; only loopback proxies are trusted (default).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

var app = builder.Build();

try
{
    // Order matters: forwarded headers → correlation id (so every later log line carries it) → errors → security → logging → auth.
    app.UseForwardedHeaders();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    app.MapMoveWellHealthEndpoints();
    app.MapAuthEndpoints();

    await app.Services.InitializeMoveWellDatabaseAsync();

    await app.RunAsync();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "MoveWell.Api terminated unexpectedly.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

/// <summary>Entry point marker for WebApplicationFactory in smoke tests.</summary>
public partial class Program;
