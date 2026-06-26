using Microsoft.Extensions.Logging;
using Nexus.SharedKernel.Events;

namespace Nexus.BuildingBlocks.EntityFrameworkCore.Outbox;

/// <summary>
/// Default <see cref="IEventPublisher"/> used when no message broker is configured.
/// Events are still durably stored in the outbox; this publisher just logs them.
/// </summary>
public sealed class LoggingEventPublisher : IEventPublisher
{
    private readonly ILogger<LoggingEventPublisher> _logger;

    public LoggingEventPublisher(ILogger<LoggingEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Integration event {EventName} ({EventId}) from {Source} for tenant {TenantId}.",
            integrationEvent.EventName,
            integrationEvent.EventId,
            integrationEvent.SourceService,
            integrationEvent.TenantId);
        return Task.CompletedTask;
    }
}
