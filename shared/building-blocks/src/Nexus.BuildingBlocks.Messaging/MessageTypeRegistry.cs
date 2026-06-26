using System.Reflection;
using Nexus.SharedKernel.Events;

namespace Nexus.BuildingBlocks.Messaging;

public sealed class MessageTypeRegistry
{
    private readonly Dictionary<string, Type> _types = new(StringComparer.Ordinal);

    public MessageTypeRegistry(IEnumerable<Assembly> assemblies)
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

    public Type? Resolve(string eventName) => _types.TryGetValue(eventName, out var type) ? type : null;
}
