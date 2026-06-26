using System.Collections.Concurrent;
using Nexus.SharedKernel.Events;

namespace Nexus.BuildingBlocks.Events;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentQueue<IIntegrationEvent> _publishedEvents = new();

    public IReadOnlyCollection<IIntegrationEvent> PublishedEvents => _publishedEvents.ToArray();

    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        _publishedEvents.Enqueue(integrationEvent);
        return Task.CompletedTask;
    }
}
