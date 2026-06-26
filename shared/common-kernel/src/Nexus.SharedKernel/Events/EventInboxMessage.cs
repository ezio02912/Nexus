namespace Nexus.SharedKernel.Events;

public sealed class EventInboxMessage
{
    public Guid EventId { get; init; }
    public string EventName { get; init; } = string.Empty;
    public string SourceService { get; init; } = string.Empty;
    public string PayloadJson { get; init; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public string? Error { get; set; }
}
