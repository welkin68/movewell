namespace MoveWell.Application.Abstractions.Security;

public interface IOtpCodeGenerator
{
    /// <summary>A cryptographically random numeric code (latin digits).</summary>
    string Generate(int length);
}
