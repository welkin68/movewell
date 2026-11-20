namespace CookieBytes.Core.Application.Abstractions;

/// <summary>
/// Processes pending outbox messages (typically from a background worker in the host app).
/// </summary>
/// <remarks>
/// <para>
/// Implementations dequeue via <see cref="IOutboxStore.GetPendingAsync"/>, dispatch each message
/// (to a message broker or in-process handlers), then call
/// <see cref="IOutboxStore.MarkProcessedAsync"/> or <see cref="IOutboxStore.MarkFailedAsync"/>.
/// </para>
/// <para>
/// Hosts invoke <see cref="ProcessPendingAsync"/> from a <c>BackgroundService</c>, timer, or job scheduler.
/// Implementations should be safe to call repeatedly and tolerate concurrent host instances when idempotent.
/// </para>
/// <para>
/// See <c>docs/OUTBOX.md</c> for store registration, reference processors, and worker examples.
/// </para>
/// </remarks>
public interface IOutboxProcessor
{
    /// <summary>
    /// Processes all pending outbox messages.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ProcessPendingAsync(CancellationToken cancellationToken = default);
}
