namespace MoveWell.Common.Identity;

/// <summary>Authorization policy names shared by Api and Web.</summary>
public static class Policies
{
    /// <summary>Admin role only (staff management).</summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>Admin or Content (manage the exercise library).</summary>
    public const string ContentEditor = "ContentEditor";

    /// <summary>Any signed-in member.</summary>
    public const string Member = "Member";
}
