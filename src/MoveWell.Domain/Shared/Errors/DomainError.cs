using FluentResults;

namespace MoveWell.Domain.Shared.Errors;

/// <summary>
/// A business failure with a stable <see cref="Code"/>. The code is the localization key
/// (FA/EN text is resolved at the edge); <see cref="Error.Message"/> is an English fallback for logs.
/// </summary>
public sealed class DomainError : Error
{
    public const string CodeKey = "code";
    public const string KindKey = "kind";

    public DomainError(string code, string message, ErrorKind? kind = null)
        : base(message)
    {
        Code = code;
        Kind = kind ?? ErrorKind.Validation;
        Metadata.Add(CodeKey, Code);
        Metadata.Add(KindKey, Kind.Name!);
    }

    public string Code { get; }

    public ErrorKind Kind { get; }
}
