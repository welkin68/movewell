namespace MoveWell.Common.Cultures;

/// <summary>Site cultures (D-12): Persian at <c>/</c> (RTL, default), English under <c>/en</c> (LTR).</summary>
public static class Cultures
{
    public const string Fa = "fa";
    public const string En = "en";

    public const string FaCultureName = "fa-IR";
    public const string EnCultureName = "en-US";

    /// <summary>URL prefix of the English site, without slashes.</summary>
    public const string EnRoutePrefix = "en";

    public static bool IsRightToLeft(string twoLetterCode) => twoLetterCode == Fa;
}
