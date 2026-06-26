using Nexus.ApiContracts.Permissions;
using Nexus.SharedKernel.Authorization;
using Nexus.SharedKernel.Context;

namespace Nexus.BuildingBlocks.Web.Auth;

/// <summary>
/// Grants access when the permission is present among the current user's claims.
/// The wildcard "*" permission grants everything (used for the platform host role).
/// </summary>
public sealed class ClaimBasedPermissionChecker : IPermissionChecker
{
    private readonly ICurrentUser _currentUser;

    public ClaimBasedPermissionChecker(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public Task<bool> IsGrantedAsync(string permission, CancellationToken cancellationToken = default)
    {
        var granted = NexusPermissionLegacy.IsGranted(_currentUser.Permissions, permission);
        return Task.FromResult(granted);
    }
}
