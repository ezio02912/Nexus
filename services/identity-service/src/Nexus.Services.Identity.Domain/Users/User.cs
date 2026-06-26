using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Validation;

namespace Nexus.Services.Identity.Domain.Users;

public sealed class User : FullAuditedAggregateRoot<Guid>
{
    private readonly List<UserRole> _roles = [];

    private User()
    {
        UserName = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
    }

    public User(Guid id, Guid tenantId, string userName, string email, string? passwordHash, Guid? creatorId, DateTimeOffset now)
    {
        Id = id;
        UserName = NormalizeUserName(userName);
        Email = NormalizeEmail(email);
        PasswordHash = passwordHash ?? string.Empty;
        IsActive = true;
        SetCreationAudit(tenantId, creatorId, now);
    }

    public string UserName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool HasPassword => !string.IsNullOrEmpty(PasswordHash);
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    public void SetRoles(IEnumerable<string> roleNames, Guid? modifierId, DateTimeOffset now)
    {
        _roles.Clear();
        foreach (var roleName in roleNames.Select(NormalizeRoleName).Distinct())
        {
            _roles.Add(new UserRole(Guid.NewGuid(), Id, roleName));
        }

        SetModificationAudit(modifierId, now);
    }

    public void ChangePassword(string passwordHash, Guid? modifierId, DateTimeOffset now)
    {
        PasswordHash = Check.NotNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        SetModificationAudit(modifierId, now);
    }

    public void SetPassword(string passwordHash, Guid? modifierId, DateTimeOffset now)
    {
        ChangePassword(passwordHash, modifierId, now);
    }

    public static string NormalizeUserName(string userName)
    {
        return Check.Length(Check.NotNullOrWhiteSpace(userName, nameof(userName)).Trim().ToUpperInvariant(), nameof(userName), UserConsts.UserNameMaxLength, UserConsts.UserNameMinLength);
    }

    public static string NormalizeEmail(string email)
    {
        return Check.Length(Check.NotNullOrWhiteSpace(email, nameof(email)).Trim().ToLowerInvariant(), nameof(email), UserConsts.EmailMaxLength);
    }

    public static string NormalizeRoleName(string roleName)
    {
        return Check.Length(Check.NotNullOrWhiteSpace(roleName, nameof(roleName)).Trim().ToUpperInvariant(), nameof(roleName), UserConsts.RoleNameMaxLength);
    }
}
