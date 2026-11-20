namespace MoveWell.Infrastructure.Options;

public sealed class SmsOptions
{
    public const string SectionName = "Sms";

    public const string NullProvider = "Null";
    public const string DevProvider = "Dev";

    /// <summary><c>Null</c> (send nothing), <c>Dev</c> (log the code; Development/Testing only). Real provider after Q-12.</summary>
    public string Provider { get; set; } = NullProvider;
}
