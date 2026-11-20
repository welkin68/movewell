using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MoveWell.Infrastructure.Options;
using MoveWell.Infrastructure.Persistence;
using MoveWell.Infrastructure.Seeding;

namespace MoveWell.Infrastructure.Host;

public static class DatabaseInitializer
{
    /// <summary>Applies migrations when <see cref="DatabaseOptions.MigrateOnStartup"/> is on, then runs idempotent seeders.</summary>
    public static async Task InitializeMoveWellDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;

        if (provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.MigrateOnStartup)
        {
            await provider.GetRequiredService<MoveWellDbContext>().Database.MigrateAsync(cancellationToken);
        }

        await provider.GetRequiredService<IdentitySeeder>().SeedAsync(cancellationToken);
    }
}
