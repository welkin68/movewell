namespace MoveWell.Web.Models.Shared;

/// <summary>Model of the shared password input partial (show/hide button, autocomplete hint).</summary>
public sealed record PasswordField(string Id, string Name, string Label, string Autocomplete, string? Hint = null);
