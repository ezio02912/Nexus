namespace Nexus.Services.Identity.Application.Users;

/// <summary>
/// Resolves the effective permission set for a user based on the user's roles. The default
/// implementation calls the permission service; failures degrade gracefully to an empty set.
/// </summary>
public interface IUserPermissionResolver
{
    Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid tenantId, IEnumerable<string> roles, CancellationToken cancellationToken = default);
}
