using Microsoft.AspNetCore.Authorization;
using Nexus.SharedKernel.Authorization;

namespace Nexus.BuildingBlocks.Web.Auth;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    public const string PolicyPrefix = "permission:";

    private readonly IPermissionChecker _permissionChecker;

    public PermissionAuthorizationHandler(IPermissionChecker permissionChecker)
    {
        _permissionChecker = permissionChecker;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (await _permissionChecker.IsGrantedAsync(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
