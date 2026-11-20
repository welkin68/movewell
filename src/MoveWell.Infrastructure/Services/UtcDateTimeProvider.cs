using CookieBytes.Core.Application.Abstractions;

namespace MoveWell.Infrastructure.Services;

/// <summary>The one clock of the app. Handlers take time from here so tests can freeze it.</summary>
public sealed class UtcDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
