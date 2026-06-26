using System.Threading;
using Nexus.SharedKernel.Context;

namespace Nexus.BuildingBlocks.Context;

public sealed class AmbientCurrentUser : ICurrentUser
{
    private static readonly AsyncLocal<UserState?> Current = new();

    public Guid? Id => Current.Value?.Id;
    public string? UserName => Current.Value?.UserName;
    public IReadOnlyCollection<string> Permissions => Current.Value?.Permissions ?? [];
    public bool IsAuthenticated => Id.HasValue;

    public static IDisposable Change(Guid? userId, string? userName = null, IReadOnlyCollection<string>? permissions = null)
    {
        var previous = Current.Value;
        Current.Value = new UserState(userId, userName, permissions ?? []);
        return new DisposeAction(() => Current.Value = previous);
    }

    private sealed record UserState(Guid? Id, string? UserName, IReadOnlyCollection<string> Permissions);
}
