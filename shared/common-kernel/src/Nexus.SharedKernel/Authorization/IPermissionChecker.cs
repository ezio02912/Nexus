namespace Nexus.SharedKernel.Authorization;

/// <summary>
/// Resolves whether the current principal is granted a named permission.
/// </summary>
public interface IPermissionChecker
{
    Task<bool> IsGrantedAsync(string permission, CancellationToken cancellationToken = default);
}
