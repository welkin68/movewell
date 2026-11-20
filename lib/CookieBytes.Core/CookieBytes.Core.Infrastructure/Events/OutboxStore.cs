using System.Text.Json;
using CookieBytes.Core.Application.Abstractions;
using CookieBytes.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CookieBytes.Core.Infrastructure.Events;

/// <summary>
/// Default outbox store using a dedicated DbContext.
/// </summary>
public sealed class OutboxStore : IOutboxStore
{
    private readonly OutboxDbContext _dbContext;

    /// <summary>
    /// Initializes the store with an outbox database context.
    /// </summary>
    /// <param name="dbContext">The EF Core context for outbox messages.</param>
    public OutboxStore(OutboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task AddAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType());
        var message = OutboxMessage.Create(integrationEvent.EventType, payload, integrationEvent.OccurredOn);
        await _dbContext.OutboxMessages.AddAsync(message, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.OutboxMessages
            .Where(message => message.ProcessedOn == null)
            .OrderBy(message => message.OccurredOn)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task MarkProcessedAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        message.MarkProcessed(DateTime.UtcNow);
        _dbContext.OutboxMessages.Update(message);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task MarkFailedAsync(OutboxMessage message, string error, CancellationToken cancellationToken = default)
    {
        message.MarkFailed(error, DateTime.UtcNow);
        _dbContext.OutboxMessages.Update(message);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// DbContext for outbox messages when outbox is enabled.
/// </summary>
public sealed class OutboxDbContext : DbContext
{
    /// <summary>
    /// Initializes the outbox context.
    /// </summary>
    /// <param name="options">DbContext options.</param>
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the outbox messages set.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("OutboxMessages");
            builder.HasKey(message => message.Id);
            builder.Property(message => message.EventType).HasMaxLength(200).IsRequired();
            builder.Property(message => message.Payload).IsRequired();
            builder.Property(message => message.Error).HasMaxLength(2000);
        });
    }
}
