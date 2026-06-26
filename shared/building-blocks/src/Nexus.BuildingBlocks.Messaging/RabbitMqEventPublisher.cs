using System.Text.Json;
using Nexus.SharedKernel.Events;
using RabbitMQ.Client;

namespace Nexus.BuildingBlocks.Messaging;

/// <summary>
/// Publishes integration events to the topic exchange using the event name as routing key.
/// </summary>
public sealed class RabbitMqEventPublisher : IEventPublisher
{
    private readonly RabbitMqConnection _connection;

    public RabbitMqEventPublisher(RabbitMqConnection connection)
    {
        _connection = connection;
    }

    public async Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        await using var channel = await _connection.CreateChannelAsync(cancellationToken);
        var payload = JsonSerializer.SerializeToUtf8Bytes(integrationEvent, integrationEvent.GetType());
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            Persistent = true
        };

        await channel.BasicPublishAsync(
            _connection.Exchange,
            integrationEvent.EventName,
            mandatory: false,
            basicProperties: properties,
            body: new ReadOnlyMemory<byte>(payload),
            cancellationToken: cancellationToken);
    }
}
