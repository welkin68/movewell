namespace MoveWell.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Apply EF migrations at startup. True only in Development; production uses the migration bundle (backend-guide §4.3).</summary>
    public bool MigrateOnStartup { get; set; }
}
