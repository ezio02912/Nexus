namespace Nexus.SharedKernel.Events;

/// <summary>
/// Transport-level publisher used by the outbox dispatcher to push integration
/// events to the message broker. Application code should depend on <see cref="IEventBus"/>
/// (which enqueues to the durable outbox) rather than this interface.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
