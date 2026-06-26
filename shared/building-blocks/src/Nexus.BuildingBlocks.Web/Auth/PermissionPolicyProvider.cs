using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Nexus.BuildingBlocks.Web.Auth;

/// <summary>
/// Dynamically materializes "permission:{name}" authorization policies so endpoints can call
/// <c>.RequireAuthorization(NexusPolicies.Permission("X"))</c> without pre-registering each policy.
/// </summary>
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PermissionAuthorizationHandler.PolicyPrefix, StringComparison.Ordinal))
        {
            var permission = policyName[PermissionAuthorizationHandler.PolicyPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallback.GetPolicyAsync(policyName);
    }
}

public static class NexusPolicies
{
    public static string Permission(string permission) =>
        PermissionAuthorizationHandler.PolicyPrefix + permission;
}
