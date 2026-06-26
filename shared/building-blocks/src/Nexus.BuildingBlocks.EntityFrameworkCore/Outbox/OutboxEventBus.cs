using System.Text.Json;
using Nexus.SharedKernel.Events;

namespace Nexus.BuildingBlocks.EntityFrameworkCore.Outbox;

/// <summary>
/// <see cref="IEventBus"/> implementation that writes integration events into the durable
/// outbox table instead of publishing them directly. A background dispatcher relays them to
/// the broker, guaranteeing at-least-once delivery even if the broker is unavailable.
/// </summary>
public sealed class OutboxEventBus : IEventBus
{
    private readonly NexusDbContext _context;

    public OutboxEventBus(NexusDbContext context)
    {
        _context = context;
    }

    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        var message = new EventOutboxMessage
        {
            EventId = integrationEvent.EventId,
            EventName = integrationEvent.EventName,
            TenantId = integrationEvent.TenantId,
            SourceService = integrationEvent.SourceService,
            PayloadJson = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()),
            OccurredAt = integrationEvent.OccurredAt
        };

        await _context.OutboxMessages.AddAsync(message, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
