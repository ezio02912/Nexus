using Nexus.SharedKernel.Events;

namespace Nexus.EventContracts.Tenants;

public sealed record TenantCreatedIntegrationEvent(
    Guid EventId,
    Guid? TenantId,
    DateTimeOffset OccurredAt,
    string SourceService,
    string? CorrelationId,
    Guid CreatedTenantId,
    string Code,
    string Name) : IIntegrationEvent
{
    public string EventName => nameof(TenantCreatedIntegrationEvent);
}
