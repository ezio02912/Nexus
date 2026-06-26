using Nexus.EventContracts.Tenants;
using Nexus.SharedKernel.Events;

namespace Nexus.EventContracts.Tenants;

public sealed record SubscriptionChangedIntegrationEvent(
    Guid EventId,
    Guid? TenantId,
    DateTimeOffset OccurredAt,
    string SourceService,
    string? CorrelationId,
    Guid ChangedTenantId,
    string OldPlanCode,
    string NewPlanCode,
    decimal Amount) : IIntegrationEvent
{
    public string EventName => nameof(SubscriptionChangedIntegrationEvent);
}
