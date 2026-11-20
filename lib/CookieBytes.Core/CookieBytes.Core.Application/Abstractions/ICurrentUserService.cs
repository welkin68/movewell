namespace CookieBytes.Core.Application.Abstractions;

/// <summary>
/// Provides the current authenticated user identifier for auditing.
/// </summary>
public interface ICurrentUserService
{
    string UserId { get; }
}
