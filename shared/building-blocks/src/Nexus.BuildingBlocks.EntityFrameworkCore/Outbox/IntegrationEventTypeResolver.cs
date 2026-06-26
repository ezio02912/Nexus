using System.Collections.Concurrent;
using System.Reflection;
using Nexus.SharedKernel.Events;

namespace Nexus.BuildingBlocks.EntityFrameworkCore.Outbox;

/// <summary>
/// Maps an integration event name back to its CLR type so the outbox dispatcher can
/// deserialize and republish persisted payloads.
/// </summary>
public sealed class IntegrationEventTypeResolver
{
    private readonly ConcurrentDictionary<string, Type> _types = new(StringComparer.Ordinal);

    public IntegrationEventTypeResolver(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsAbstract && !type.IsInterface && typeof(IIntegrationEvent).IsAssignableFrom(type))
                {
                    _types[type.Name] = type;
                }
            }
        }
    }

    public Type? Resolve(string eventName)
    {
        return _types.TryGetValue(eventName, out var type) ? type : null;
    }
}
