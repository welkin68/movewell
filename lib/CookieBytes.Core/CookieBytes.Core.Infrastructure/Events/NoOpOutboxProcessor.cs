using CookieBytes.Core.Application.Abstractions;

namespace CookieBytes.Core.Infrastructure.Events;

/// <summary>
/// Stub outbox processor that marks pending messages processed without broker delivery.
/// Register explicitly via <see cref="DependencyInjection.ServiceCollectionExtensions.AddCookieBytesCoreOutboxProcessor{TProcessor}"/>
/// or <see cref="DependencyInjection.CookieBytesCoreOptions.RegisterNoOpOutboxProcessor"/> in Development only.
/// </summary>
public sealed class NoOpOutboxProcessor : IOutboxProcessor
{
    private readonly IOutboxStore _outboxStore;

    /// <summary>
    /// Initializes the processor with an outbox store.
    /// </summary>
    /// <param name="outboxStore">The store containing pending messages.</param>
    public NoOpOutboxProcessor(IOutboxStore outboxStore)
    {
        _outboxStore = outboxStore;
    }

    /// <inheritdoc />
    public async Task ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _outboxStore.GetPendingAsync(cancellationToken);

        foreach (var message in pending)
        {
            await _outboxStore.MarkProcessedAsync(message, cancellationToken);
        }
    }
}
