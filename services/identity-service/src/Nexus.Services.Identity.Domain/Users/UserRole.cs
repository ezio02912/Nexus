using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Validation;

namespace Nexus.Services.Identity.Domain.Users;

public sealed class UserRole : NexusEntity<Guid>
{
    private UserRole()
    {
        RoleName = string.Empty;
    }

    public UserRole(Guid id, Guid userId, string roleName)
    {
        Id = id;
        UserId = userId;
        RoleName = Check.Length(Check.NotNullOrWhiteSpace(roleName, nameof(roleName)), nameof(roleName), UserConsts.RoleNameMaxLength);
    }

    public Guid UserId { get; private set; }
    public string RoleName { get; private set; }
}
