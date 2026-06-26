namespace Nexus.SharedKernel.Events;

public interface IEventInbox
{
    Task<bool> HasProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task MarkProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);
}
