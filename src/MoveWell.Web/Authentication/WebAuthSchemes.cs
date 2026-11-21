namespace MoveWell.Web.Authentication;

/// <summary>Two independent BFF cookies (ADR-0003): a member session for the public site, a staff session for /admin.</summary>
public static class WebAuthSchemes
{
    public const string Member = "MoveWell.Member";
    public const string Staff = "MoveWell.Staff";
}
