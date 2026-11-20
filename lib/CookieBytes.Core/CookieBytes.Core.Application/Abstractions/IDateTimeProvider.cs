namespace CookieBytes.Core.Application.Abstractions;

/// <summary>
/// Abstraction over system clock for testability.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
