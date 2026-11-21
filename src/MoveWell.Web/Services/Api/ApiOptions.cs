namespace MoveWell.Web.Services.Api;

public sealed class ApiOptions
{
    public const string SectionName = "Api";

    /// <summary>The Api root, e.g. <c>http://localhost:5100/</c>. On the server it is the internal binding (ADR-0001).</summary>
    public string BaseUrl { get; set; } = "http://localhost:5100/";

    public int TimeoutSeconds { get; set; } = 10;
}
