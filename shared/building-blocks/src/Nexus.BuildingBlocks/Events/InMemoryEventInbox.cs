using System.Collections.Concurrent;
using Nexus.SharedKernel.Events;

namespace Nexus.BuildingBlocks.Events;

public sealed class InMemoryEventInbox : IEventInbox
{
    private readonly ConcurrentDictionary<Guid, DateTimeOffset> _processedEvents = new();

    public Task<bool> HasProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_processedEvents.ContainsKey(eventId));
    }

    public Task MarkProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        _processedEvents[eventId] = DateTimeOffset.UtcNow;
        return Task.CompletedTask;
    }
}
