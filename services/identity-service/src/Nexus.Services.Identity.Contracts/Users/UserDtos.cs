using Nexus.ApiContracts.Dtos;

namespace Nexus.Services.Identity.Contracts.Users;

public sealed class GetUsersInput : PagedAndSortedResultRequestDto
{
    public Guid? TenantId { get; init; }
    public string? FilterText { get; init; }
}

public sealed class CreateUserDto
{
    public required Guid TenantId { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
}

public sealed class LoginDto
{
    public required Guid TenantId { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
}

public sealed class LoginResultDto
{
    public required Guid UserId { get; init; }
    public required Guid TenantId { get; init; }
    public required string AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
}

public sealed class RefreshTokenDto
{
    public required string RefreshToken { get; init; }
}

public sealed class ChangeUserRolesDto
{
    public IReadOnlyCollection<string> Roles { get; init; } = [];
}

public sealed class UserDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
    public string ConcurrencyStamp { get; init; } = string.Empty;
}
