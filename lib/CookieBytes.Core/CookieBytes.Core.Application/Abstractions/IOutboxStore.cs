using CookieBytes.Core.Domain.Interfaces;

namespace CookieBytes.Core.Application.Abstractions;

/// <summary>
/// Port for persisting and processing integration event outbox messages.
/// </summary>
public interface IOutboxStore
{
    /// <summary>
    /// Adds an integration event to the outbox.
    /// </summary>
    /// <param name="integrationEvent">The event to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns messages that have not yet been processed.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as successfully processed.
    /// </summary>
    /// <param name="message">The message to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task MarkProcessedAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as failed with an error.
    /// </summary>
    /// <param name="message">The message to update.</param>
    /// <param name="error">The error description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task MarkFailedAsync(OutboxMessage message, string error, CancellationToken cancellationToken = default);
}
