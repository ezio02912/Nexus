using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nexus.BuildingBlocks.EntityFrameworkCore.Outbox;
using Nexus.BuildingBlocks.EntityFrameworkCore.UnitOfWork;
using Nexus.SharedKernel.Events;
using Nexus.SharedKernel.UnitOfWork;

namespace Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;

public static class NexusEfCoreExtensions
{
    /// <summary>
    /// Registers a PostgreSQL-backed Nexus DbContext together with the durable outbox
    /// (event bus + background dispatcher) and a unit of work.
    /// </summary>
    public static IServiceCollection AddNexusEfCore<TDbContext>(
        this IServiceCollection services,
        string connectionString,
        params Assembly[] eventContractAssemblies)
        where TDbContext : NexusDbContext
    {
        services.AddDbContext<TDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(TDbContext).Assembly.FullName)));

        services.AddScoped<NexusDbContext>(sp => sp.GetRequiredService<TDbContext>());
        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();
        services.AddScoped<IEventBus, OutboxEventBus>();

        var assemblies = eventContractAssemblies.Length > 0
            ? eventContractAssemblies
            : [typeof(IIntegrationEvent).Assembly];
        services.AddSingleton(new IntegrationEventTypeResolver(assemblies));

        services.TryAddSingleton<IEventPublisher, LoggingEventPublisher>();
        services.AddHostedService<OutboxDispatcherHostedService>();

        return services;
    }
}
