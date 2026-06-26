using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.SharedKernel.Events;

namespace Nexus.BuildingBlocks.Messaging;

public static class NexusMessagingExtensions
{
    /// <summary>
    /// Registers a RabbitMQ-backed <see cref="IEventPublisher"/> (used by the outbox dispatcher).
    /// </summary>
    public static IServiceCollection AddNexusRabbitMqPublisher(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddNexusRabbitMqCore(configuration);
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        return services;
    }

    /// <summary>
    /// Registers the RabbitMQ consumer hosted service that fans events out to registered handlers.
    /// </summary>
    public static IServiceCollection AddNexusRabbitMqConsumer(
        this IServiceCollection services,
        IConfiguration configuration,
        string queueName,
        params Assembly[] eventContractAssemblies)
    {
        services.AddNexusRabbitMqCore(configuration);
        services.Configure<RabbitMqConsumerOptions>(o => o.QueueName = queueName);

        var assemblies = eventContractAssemblies.Length > 0
            ? eventContractAssemblies
            : [typeof(IIntegrationEvent).Assembly];
        services.AddSingleton(new MessageTypeRegistry(assemblies));
        services.AddHostedService<RabbitMqConsumerHostedService>();
        return services;
    }

    public static IServiceCollection AddIntegrationEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : IIntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
    {
        services.AddScoped<IIntegrationEventHandler<TEvent>, THandler>();
        return services;
    }

    private static IServiceCollection AddNexusRabbitMqCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<RabbitMqConnection>();
        return services;
    }
}
