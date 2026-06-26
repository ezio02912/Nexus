namespace Nexus.SharedKernel.Events;

public interface IEventOutbox
{
    Task EnqueueAsync(IIntegrationEvent integrationEvent, string payloadJson, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventOutboxMessage>> GetUnpublishedAsync(int maxResultCount = 100, CancellationToken cancellationToken = default);
    Task MarkPublishedAsync(Guid eventId, CancellationToken cancellationToken = default);
}
