using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Nexus.Services.Identity.Application.Users;

namespace Nexus.Services.Identity.Infrastructure.Users;

/// <summary>
/// Resolves permissions by querying the permission service for each role. Network failures
/// degrade gracefully to an empty permission set so login still succeeds.
/// </summary>
public sealed class HttpUserPermissionResolver : IUserPermissionResolver
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpUserPermissionResolver> _logger;

    public HttpUserPermissionResolver(HttpClient httpClient, ILogger<HttpUserPermissionResolver> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid tenantId, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        var permissions = new HashSet<string>(StringComparer.Ordinal);

        foreach (var role in roles)
        {
            await AddRolePermissionsAsync(role, permissions, cancellationToken);
            await AddRolePermissionsAsync($"{tenantId:N}:{role}", permissions, cancellationToken);
        }

        return permissions;
    }

    private async Task AddRolePermissionsAsync(string role, HashSet<string> permissions, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<RolePermissionResponse>(
                $"/api/roles/{Uri.EscapeDataString(role)}/permissions",
                cancellationToken);

            if (result?.Permissions is not null)
            {
                foreach (var permission in result.Permissions)
                {
                    permissions.Add(permission);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve permissions for role {Role}.", role);
        }
    }

    private sealed record RolePermissionResponse(string RoleName, string[] Permissions);
}
