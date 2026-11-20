using MoveWell.Api.Endpoints.Health;
using MoveWell.Api.Middleware;
using MoveWell.Infrastructure.DependencyInjection;
using MoveWell.Infrastructure.Host;
using MoveWell.Infrastructure.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
builder.AddMoveWellSerilog("MoveWell.Api");

builder.Services.AddProblemDetails();
builder.Services.AddMoveWellInfrastructure(builder.Configuration, builder.Environment);

var app = builder.Build();

try
{
    // Order matters: correlation id first so every later log line carries it.
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseExceptionHandler();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseSerilogRequestLogging();

    app.MapMoveWellHealthEndpoints();

    if (!app.Environment.IsEnvironment("Testing"))
    {
        await app.Services.InitializeMoveWellDatabaseAsync();
    }

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
