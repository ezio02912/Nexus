namespace Nexus.SharedKernel.Events;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    string EventName { get; }
    Guid? TenantId { get; }
    DateTimeOffset OccurredAt { get; }
    string SourceService { get; }
    string? CorrelationId { get; }
}
