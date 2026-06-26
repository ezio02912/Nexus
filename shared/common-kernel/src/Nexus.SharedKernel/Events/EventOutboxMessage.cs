namespace Nexus.SharedKernel.Events;

public sealed class EventOutboxMessage
{
    public Guid EventId { get; init; }
    public string EventName { get; init; } = string.Empty;
    public Guid? TenantId { get; init; }
    public string SourceService { get; init; } = string.Empty;
    public string PayloadJson { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string? Error { get; set; }
}
