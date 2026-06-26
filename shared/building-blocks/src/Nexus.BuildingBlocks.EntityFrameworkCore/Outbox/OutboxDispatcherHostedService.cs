using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexus.SharedKernel.Events;

namespace Nexus.BuildingBlocks.EntityFrameworkCore.Outbox;

/// <summary>
/// Periodically relays unpublished outbox messages to the broker via <see cref="IEventPublisher"/>.
/// </summary>
public sealed class OutboxDispatcherHostedService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 50;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcherHostedService> _logger;

    public OutboxDispatcherHostedService(IServiceScopeFactory scopeFactory, ILogger<OutboxDispatcherHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox dispatch iteration failed.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task DispatchBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NexusDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var resolver = scope.ServiceProvider.GetRequiredService<IntegrationEventTypeResolver>();

        var pending = await context.OutboxMessages
            .Where(x => x.PublishedAt == null)
            .OrderBy(x => x.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return;
        }

        foreach (var message in pending)
        {
            try
            {
                var type = resolver.Resolve(message.EventName);
                if (type is null)
                {
                    message.Error = $"Unknown event type '{message.EventName}'.";
                    message.PublishedAt = DateTimeOffset.UtcNow;
                    continue;
                }

                if (JsonSerializer.Deserialize(message.PayloadJson, type) is IIntegrationEvent integrationEvent)
                {
                    await publisher.PublishAsync(integrationEvent, cancellationToken);
                }

                message.PublishedAt = DateTimeOffset.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                _logger.LogError(ex, "Failed to publish outbox message {EventId}.", message.EventId);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
