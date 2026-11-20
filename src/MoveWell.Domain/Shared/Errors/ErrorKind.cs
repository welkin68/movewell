using CookieBytes.Core.Domain.SeedWork;

namespace MoveWell.Domain.Shared.Errors;

/// <summary>Category of a failure; the Api maps it to an HTTP status (400/404/409/403/401/429).</summary>
public sealed class ErrorKind : Enumeration
{
    public static readonly ErrorKind Validation = new(1, nameof(Validation));
    public static readonly ErrorKind NotFound = new(2, nameof(NotFound));
    public static readonly ErrorKind Conflict = new(3, nameof(Conflict));
    public static readonly ErrorKind Forbidden = new(4, nameof(Forbidden));
    public static readonly ErrorKind Unauthorized = new(5, nameof(Unauthorized));
    public static readonly ErrorKind TooManyRequests = new(6, nameof(TooManyRequests));

    private ErrorKind(int value, string name)
        : base(value, name)
    {
    }
}
