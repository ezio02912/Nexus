using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexus.SharedKernel.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Nexus.BuildingBlocks.Messaging;

/// <summary>
/// Subscribes to all events on the topic exchange and dispatches each one to every registered
/// <see cref="IIntegrationEventHandler{TEvent}"/>.
/// </summary>
public sealed class RabbitMqConsumerHostedService : BackgroundService
{
    private readonly RabbitMqConnection _connection;
    private readonly MessageTypeRegistry _registry;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMqConsumerHostedService> _logger;
    private readonly string _queueName;
    private IChannel? _channel;

    public RabbitMqConsumerHostedService(
        RabbitMqConnection connection,
        MessageTypeRegistry registry,
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqConsumerOptions> options,
        ILogger<RabbitMqConsumerHostedService> logger)
    {
        _connection = connection;
        _registry = registry;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _queueName = options.Value.QueueName;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _channel = await _connection.CreateChannelAsync(stoppingToken);
            await _channel.QueueDeclareAsync(_queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(_queueName, _connection.Exchange, routingKey: "#", cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnReceivedAsync;
            await _channel.BasicConsumeAsync(_queueName, autoAck: false, consumer, stoppingToken);

            _logger.LogInformation("RabbitMQ consumer listening on queue {Queue}.", _queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ consumer.");
        }
    }

    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        var eventName = args.RoutingKey;
        try
        {
            var type = _registry.Resolve(eventName);
            if (type is null)
            {
                _logger.LogWarning("No type registered for event {EventName}; skipping.", eventName);
                if (_channel is not null)
                {
                    await _channel.BasicAckAsync(args.DeliveryTag, multiple: false);
                }

                return;
            }

            var json = Encoding.UTF8.GetString(args.Body.Span);
            if (JsonSerializer.Deserialize(json, type) is IIntegrationEvent integrationEvent)
            {
                await DispatchAsync(type, integrationEvent);
            }

            if (_channel is not null)
            {
                await _channel.BasicAckAsync(args.DeliveryTag, multiple: false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle event {EventName}.", eventName);
            if (_channel is not null)
            {
                await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
            }
        }
    }

    private async Task DispatchAsync(Type eventType, IIntegrationEvent integrationEvent)
    {
        using var scope = _scopeFactory.CreateScope();
        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handlers = scope.ServiceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            var method = handlerType.GetMethod("HandleAsync");
            if (method?.Invoke(handler, [integrationEvent, CancellationToken.None]) is Task task)
            {
                await task;
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}

public sealed class RabbitMqConsumerOptions
{
    public string QueueName { get; set; } = "nexus.worker";
}
