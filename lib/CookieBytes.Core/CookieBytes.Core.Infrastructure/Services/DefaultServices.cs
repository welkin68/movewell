using CookieBytes.Core.Application.Abstractions;

namespace CookieBytes.Core.Infrastructure.Services;

/// <summary>
/// Development-only UTC clock. Registered when <see cref="DependencyInjection.CookieBytesCoreOptions.RegisterDefaultAuditServices"/> is true.
/// </summary>
internal sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

/// <summary>
/// Development-only audit user returning <c>"System"</c>.
/// Registered when <see cref="DependencyInjection.CookieBytesCoreOptions.RegisterDefaultAuditServices"/> is true.
/// Register a production <see cref="ICurrentUserService"/> instead.
/// </summary>
internal sealed class SystemCurrentUserService : ICurrentUserService
{
    public string UserId => "System";
}
