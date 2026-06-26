using Nexus.SharedKernel.Domain;

namespace Nexus.Services.Permission.Api.Persistence;

public sealed class RolePermission : NexusEntity<Guid>
{
    private RolePermission()
    {
        RoleName = string.Empty;
        Permission = string.Empty;
    }

    public RolePermission(Guid id, string roleName, string permission)
    {
        Id = id;
        RoleName = roleName;
        Permission = permission;
    }

    public string RoleName { get; private set; }
    public string Permission { get; private set; }
}
