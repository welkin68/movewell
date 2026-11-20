using System.Reflection;
using CookieBytes.Core.Application.Abstractions;

namespace CookieBytes.Core.Infrastructure.DependencyInjection;

/// <summary>
/// Configuration options for CookieBytes.Core services.
/// </summary>
public sealed class CookieBytesCoreOptions
{
    /// <summary>
    /// Gets or sets whether FluentValidation runs in the MediatR pipeline.
    /// </summary>
    public bool EnableValidationPipeline { get; set; } = true;

    /// <summary>
    /// Gets or sets how validation failures are surfaced when <see cref="EnableValidationPipeline"/> is true.
    /// Default is <see cref="CookieBytesCoreValidationMode.Throw"/>.
    /// </summary>
    public CookieBytesCoreValidationMode ValidationMode { get; set; } = CookieBytesCoreValidationMode.Throw;

    /// <summary>
    /// Gets or sets whether request logging runs in the MediatR pipeline.
    /// </summary>
    public bool EnableLoggingPipeline { get; set; } = true;

    /// <summary>
    /// Gets or sets whether domain events are dispatched after save.
    /// </summary>
    public bool EnableDomainEventDispatch { get; set; } = true;

    /// <summary>
    /// When true, registers development-only <see cref="IDateTimeProvider"/> and
    /// <see cref="ICurrentUserService"/> implementations (user id <c>"System"</c>).
    /// For local Development only — never enable in Production. See docs/AUDIT.md.
    /// </summary>
    public bool RegisterDefaultAuditServices { get; set; }

    /// <summary>
    /// When true (default), validates audit options at host startup.
    /// Fails in non-Development environments if <see cref="RegisterDefaultAuditServices"/> is true.
    /// </summary>
    public bool ValidateAuditServicesOnStart { get; set; } = true;

    /// <summary>
    /// Gets or sets whether outbox services are registered for integration events.
    /// </summary>
    public bool EnableOutbox { get; set; }

    /// <summary>
    /// When true with <see cref="EnableOutbox"/>, registers <see cref="Events.NoOpOutboxProcessor"/>.
    /// Development/testing only — marks messages processed without broker delivery. See docs/OUTBOX.md.
    /// </summary>
    public bool RegisterNoOpOutboxProcessor { get; set; }

    /// <summary>
    /// When true (default), fails startup if <see cref="EnableOutbox"/> is true but no processor was registered.
    /// </summary>
    public bool ValidateOutboxProcessorOnStart { get; set; } = true;

    /// <summary>
    /// Gets or sets the connection string for <see cref="Events.OutboxDbContext"/>.
    /// When set with <see cref="EnableOutbox"/>, the outbox context is registered automatically.
    /// </summary>
    public string? OutboxConnectionString { get; set; }

    /// <summary>
    /// Gets assemblies scanned for MediatR handlers and FluentValidation validators.
    /// </summary>
    public IList<Assembly> MediatRAssemblies { get; } = [];

    /// <summary>
    /// Gets whether <see cref="Events.OutboxDbContext"/> was registered via connection string or <see cref="ServiceCollectionExtensions.AddCookieBytesCoreOutbox"/>.
    /// </summary>
    internal bool OutboxDbContextRegistered { get; set; }

    /// <summary>
    /// Gets whether <see cref="IOutboxProcessor"/> was registered via <see cref="RegisterNoOpOutboxProcessor"/> or <see cref="ServiceCollectionExtensions.AddCookieBytesCoreOutboxProcessor{TProcessor}"/>.
    /// </summary>
    internal bool OutboxProcessorRegistered { get; set; }

    /// <summary>
    /// Gets whether FluentResults integration was registered via CookieBytes.Core.Results.
    /// </summary>
    internal bool FluentResultsRegistered { get; set; }
}
