using Nexus.SharedKernel.Events;

namespace Nexus.EventContracts.Identity;

public sealed record UserCreatedIntegrationEvent(
    Guid EventId,
    Guid? TenantId,
    DateTimeOffset OccurredAt,
    string SourceService,
    string? CorrelationId,
    Guid UserId,
    string UserName,
    string Email) : IIntegrationEvent
{
    public string EventName => nameof(UserCreatedIntegrationEvent);
}
