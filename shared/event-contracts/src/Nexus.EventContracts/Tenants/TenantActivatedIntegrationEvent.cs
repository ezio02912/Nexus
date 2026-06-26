using Nexus.SharedKernel.Events;

namespace Nexus.EventContracts.Tenants;

public sealed record TenantActivatedIntegrationEvent(
    Guid EventId,
    Guid? TenantId,
    DateTimeOffset OccurredAt,
    string SourceService,
    string? CorrelationId,
    Guid ActivatedTenantId,
    string Code,
    string Name) : IIntegrationEvent
{
    public string EventName => nameof(TenantActivatedIntegrationEvent);
}
