using Nexus.SharedKernel.Events;

namespace Nexus.EventContracts.Identity;

public sealed record UserRoleChangedIntegrationEvent(
    Guid EventId,
    Guid? TenantId,
    DateTimeOffset OccurredAt,
    string SourceService,
    string? CorrelationId,
    Guid UserId,
    IReadOnlyCollection<string> Roles) : IIntegrationEvent
{
    public string EventName => nameof(UserRoleChangedIntegrationEvent);
}
