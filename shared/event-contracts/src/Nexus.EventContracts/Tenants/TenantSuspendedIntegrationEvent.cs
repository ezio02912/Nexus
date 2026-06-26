using Nexus.SharedKernel.Events;

namespace Nexus.EventContracts.Tenants;

public sealed record TenantSuspendedIntegrationEvent(
    Guid EventId,
    Guid? TenantId,
    DateTimeOffset OccurredAt,
    string SourceService,
    string? CorrelationId,
    Guid SuspendedTenantId,
    string Code,
    string Name) : IIntegrationEvent
{
    public string EventName => nameof(TenantSuspendedIntegrationEvent);
}
