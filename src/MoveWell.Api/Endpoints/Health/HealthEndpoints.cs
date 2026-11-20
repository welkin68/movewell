using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MoveWell.Infrastructure.DependencyInjection;

namespace MoveWell.Api.Endpoints.Health;

public static class HealthEndpoints
{
    /// <summary>
    /// <c>/health/live</c>: the process answers (no dependencies checked).
    /// <c>/health/ready</c>: SQL Server is reachable — used after deploy and by uptime monitors.
    /// </summary>
    public static IEndpointRouteBuilder MapMoveWellHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(InfrastructureServiceCollectionExtensions.ReadyTag),
        });

        return endpoints;
    }
}
