using System.Text.Json;
using Nexus.Services.Crm.Infrastructure.Persistence;
using Nexus.SharedKernel.Events;

namespace Nexus.Services.Crm.Infrastructure.Outbox;

/// <summary>
/// Writes integration events into the CRM outbox table so they are dispatched
/// with the same durability guarantees as other Nexus services.
/// </summary>
public sealed class CrmEventPublisher
{
    private const string SourceService = "crm-service";

    private readonly CrmDbContext _context;

    public CrmEventPublisher(CrmDbContext context)
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
            SourceService = string.IsNullOrWhiteSpace(integrationEvent.SourceService)
                ? SourceService
                : integrationEvent.SourceService,
            PayloadJson = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()),
            OccurredAt = integrationEvent.OccurredAt
        };

        await _context.OutboxMessages.AddAsync(message, cancellationToken);
    }
}
