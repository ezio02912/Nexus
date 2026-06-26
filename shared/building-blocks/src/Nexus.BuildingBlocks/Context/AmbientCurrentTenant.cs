using System.Threading;
using Nexus.SharedKernel.Context;

namespace Nexus.BuildingBlocks.Context;

public sealed class AmbientCurrentTenant : ICurrentTenant
{
    private static readonly AsyncLocal<TenantState?> Current = new();

    public Guid? Id => Current.Value?.Id;
    public string? Name => Current.Value?.Name;
    public bool IsAvailable => Id.HasValue;

    public static IDisposable Change(Guid? tenantId, string? name = null)
    {
        var previous = Current.Value;
        Current.Value = new TenantState(tenantId, name);
        return new DisposeAction(() => Current.Value = previous);
    }

    private sealed record TenantState(Guid? Id, string? Name);
}
