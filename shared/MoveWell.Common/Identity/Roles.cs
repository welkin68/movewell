namespace MoveWell.Common.Identity;

/// <summary>Identity role names (ADR-0003). Staff: Admin, Content. Public: Member.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Content = "Content";
    public const string Member = "Member";

    public static IReadOnlyList<string> All { get; } = [Admin, Content, Member];
}
