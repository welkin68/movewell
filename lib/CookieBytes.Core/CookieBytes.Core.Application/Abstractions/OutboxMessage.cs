namespace CookieBytes.Core.Application.Abstractions;

/// <summary>
/// Outbox message persisted alongside domain changes for reliable integration event delivery.
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>
    /// Gets the unique message identifier.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Gets the integration event type name.
    /// </summary>
    public string EventType { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the serialized event payload.
    /// </summary>
    public string Payload { get; private set; } = string.Empty;

    /// <summary>
    /// Gets when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; private set; }

    /// <summary>
    /// Gets when the message was processed, if applicable.
    /// </summary>
    public DateTime? ProcessedOn { get; private set; }

    /// <summary>
    /// Gets the last processing error, if the message failed.
    /// </summary>
    public string? Error { get; private set; }

    private OutboxMessage()
    {
    }

    /// <summary>
    /// Creates a new outbox message for the given event data.
    /// </summary>
    /// <param name="eventType">The integration event type name.</param>
    /// <param name="payload">The serialized event payload.</param>
    /// <param name="occurredOn">When the event occurred.</param>
    public static OutboxMessage Create(string eventType, string payload, DateTime occurredOn) =>
        new()
        {
            EventType = eventType,
            Payload = payload,
            OccurredOn = occurredOn
        };

    /// <summary>
    /// Marks the message as successfully processed.
    /// </summary>
    /// <param name="processedOn">The processing timestamp.</param>
    public void MarkProcessed(DateTime processedOn)
    {
        ProcessedOn = processedOn;
        Error = null;
    }

    /// <summary>
    /// Marks the message as failed with an error.
    /// </summary>
    /// <param name="error">The error description.</param>
    /// <param name="processedOn">The processing timestamp.</param>
    public void MarkFailed(string error, DateTime processedOn)
    {
        ProcessedOn = processedOn;
        Error = error;
    }
}
