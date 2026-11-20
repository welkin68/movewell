namespace CookieBytes.Core.Infrastructure.DependencyInjection;

/// <summary>
/// Controls how MediatR FluentValidation pipeline failures are surfaced.
/// </summary>
public enum CookieBytesCoreValidationMode
{
    /// <summary>
    /// Validation failures throw <see cref="FluentValidation.ValidationException"/> (default).
    /// </summary>
    Throw,

    /// <summary>
    /// Validation failures return failed Result or Result&lt;T&gt; (requires CookieBytes.Core.Results). See docs/RESULTS.md.
    /// </summary>
    FluentResult
}
