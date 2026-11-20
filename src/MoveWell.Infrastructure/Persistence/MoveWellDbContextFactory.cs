using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MoveWell.Infrastructure.Persistence;

/// <summary>
/// Used only by <c>dotnet ef</c> at design time. Reads <c>MOVEWELL_DESIGN_CONNECTION</c>, otherwise LocalDB.
/// </summary>
public sealed class MoveWellDbContextFactory : IDesignTimeDbContextFactory<MoveWellDbContext>
{
    private const string LocalDbFallback =
        "Server=(localdb)\\MSSQLLocalDB;Database=MoveWell_Dev;Trusted_Connection=True;TrustServerCertificate=True";

    public MoveWellDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("MOVEWELL_DESIGN_CONNECTION") ?? LocalDbFallback;
        var options = new DbContextOptionsBuilder<MoveWellDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory"))
            .Options;

        return new MoveWellDbContext(options);
    }
}
