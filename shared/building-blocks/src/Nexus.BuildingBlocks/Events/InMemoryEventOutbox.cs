using System.Collections.Concurrent;
using Nexus.SharedKernel.Events;

namespace Nexus.BuildingBlocks.Events;

public sealed class InMemoryEventOutbox : IEventOutbox
{
    private readonly ConcurrentDictionary<Guid, EventOutboxMessage> _messages = new();

    public Task EnqueueAsync(IIntegrationEvent integrationEvent, string payloadJson, CancellationToken cancellationToken = default)
    {
        _messages[integrationEvent.EventId] = new EventOutboxMessage
        {
            EventId = integrationEvent.EventId,
            EventName = integrationEvent.EventName,
            TenantId = integrationEvent.TenantId,
            SourceService = integrationEvent.SourceService,
            PayloadJson = payloadJson,
            OccurredAt = integrationEvent.OccurredAt
        };

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<EventOutboxMessage>> GetUnpublishedAsync(int maxResultCount = 100, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EventOutboxMessage> messages = _messages.Values
            .Where(x => x.PublishedAt is null)
            .OrderBy(x => x.OccurredAt)
            .Take(maxResultCount)
            .ToArray();

        return Task.FromResult(messages);
    }

    public Task MarkPublishedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        if (_messages.TryGetValue(eventId, out var message))
        {
            message.PublishedAt = DateTimeOffset.UtcNow;
        }

        return Task.CompletedTask;
    }
}
