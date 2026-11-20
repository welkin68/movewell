using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace MoveWell.Infrastructure.Logging;

public static class SerilogSetup
{
    /// <summary>
    /// Console + daily rolling file (14 days) under <c>Logging:Directory</c> (default <c>logs/</c>), enriched with LogContext
    /// (correlation id). Extra levels/sinks can be set in the <c>Serilog</c> config section.
    /// </summary>
    public static WebApplicationBuilder AddMoveWellSerilog(this WebApplicationBuilder builder, string applicationName)
    {
        var directory = builder.Configuration["Logging:Directory"] ?? "logs";
        var filePrefix = applicationName.ToLowerInvariant().Replace('.', '-');

        builder.Services.AddSerilog((services, configuration) => configuration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command",
                builder.Environment.IsDevelopment() ? LogEventLevel.Information : LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName)
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(directory, filePrefix + "-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {SourceContext} {Message:lj}{NewLine}{Exception}"));

        return builder;
    }
}
