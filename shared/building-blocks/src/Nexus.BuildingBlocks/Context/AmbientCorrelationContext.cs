using System.Threading;
using Nexus.SharedKernel.Context;

namespace Nexus.BuildingBlocks.Context;

public sealed class AmbientCorrelationContext : ICorrelationContext
{
    private static readonly AsyncLocal<string?> Current = new();

    public string? CorrelationId => Current.Value;

    public static IDisposable Change(string? correlationId)
    {
        var previous = Current.Value;
        Current.Value = correlationId;
        return new DisposeAction(() => Current.Value = previous);
    }
}
