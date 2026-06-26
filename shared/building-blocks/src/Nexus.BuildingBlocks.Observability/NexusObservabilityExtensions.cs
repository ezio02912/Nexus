using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Nexus.BuildingBlocks.Observability;

public static class NexusObservabilityExtensions
{
    /// <summary>
    /// Configures structured Serilog output and Prometheus metrics for an ASP.NET Core host.
    /// </summary>
    public static WebApplicationBuilder AddNexusObservability(this WebApplicationBuilder builder, string serviceName)
    {
        ConfigureSerilog(serviceName);

        builder.Host.UseSerilog();

        return builder;
    }

    /// <summary>
    /// Configures structured Serilog output for a generic .NET worker host.
    /// </summary>
    public static HostApplicationBuilder AddNexusWorkerObservability(this HostApplicationBuilder builder, string serviceName)
    {
        ConfigureSerilog(serviceName);
        builder.Services.AddSerilog();
        return builder;
    }

    /// <summary>
    /// Maps Prometheus scrape endpoint and standard health routes.
    /// </summary>
    public static WebApplication MapNexusObservability(this WebApplication app)
    {
        app.UseHttpMetrics();
        app.MapMetrics();
        return app;
    }

    private static void ConfigureSerilog(string serviceName)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Service", serviceName)
            .WriteTo.Console(new CompactJsonFormatter())
            .CreateLogger();
    }
}
