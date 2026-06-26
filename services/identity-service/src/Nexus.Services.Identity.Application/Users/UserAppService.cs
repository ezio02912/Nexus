using Nexus.ApiContracts.Dtos;
using Nexus.BuildingBlocks.Application;
using Nexus.EventContracts.Identity;
using Nexus.Services.Identity.Contracts.Users;
using Nexus.Services.Identity.Domain.Users;
using Nexus.SharedKernel.Auditing;
using Nexus.SharedKernel.Context;
using Nexus.SharedKernel.Events;
using Nexus.SharedKernel.Exceptions;

namespace Nexus.Services.Identity.Application.Users;

public sealed class UserAppService : NexusAppServiceBase, IUserAppService
{
    private const string ServiceName = "identity-service";
    private readonly IUserRepository _userRepository;
    private readonly UserManager _userManager;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEventBus _eventBus;
    private readonly IAuditWriter _auditWriter;

    public UserAppService(IUserRepository userRepository, UserManager userManager, IPasswordHasher passwordHasher, ITokenService tokenService, IEventBus eventBus, IAuditWriter auditWriter, ICurrentTenant currentTenant, ICurrentUser currentUser, ICorrelationContext correlationContext) : base(currentTenant, currentUser, correlationContext)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _eventBus = eventBus;
        _auditWriter = auditWriter;
    }

    public async Task<PagedResultDto<UserDto>> GetListAsync(GetUsersInput input, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetListAsync(input.SkipCount, input.MaxResultCount, input.Sorting, cancellationToken);
        if (input.TenantId.HasValue)
        {
            users = users.Where(x => x.TenantId == input.TenantId.Value).ToArray();
        }

        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            users = users.Where(x => x.UserName.Contains(input.FilterText, StringComparison.OrdinalIgnoreCase) || x.Email.Contains(input.FilterText, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        return new PagedResultDto<UserDto>
        {
            TotalCount = users.Count,
            Items = users.Select(MapToDto).ToArray()
        };
    }

    public async Task<UserDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return MapToDto(await _userRepository.GetAsync(id, cancellationToken));
    }

    public async Task<UserDto> CreateAsync(CreateUserDto input, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.CreateAsync(input.TenantId, input.UserName, input.Email, input.Password, input.Roles, cancellationToken);
        await _auditWriter.WriteAsync(new AuditLogEntry(Guid.NewGuid(), user.TenantId, CurrentUser.Id, ServiceName, nameof(User), user.Id.ToString(), AuditAction.Create, "User created.", CorrelationContext.CorrelationId, DateTimeOffset.UtcNow), cancellationToken);
        await _eventBus.PublishAsync(new UserCreatedIntegrationEvent(Guid.NewGuid(), user.TenantId, DateTimeOffset.UtcNow, ServiceName, CorrelationContext.CorrelationId, user.Id, user.UserName, user.Email), cancellationToken);
        return MapToDto(user);
    }

    public async Task<LoginResultDto> LoginAsync(LoginDto input, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByUserNameAsync(input.TenantId, User.NormalizeUserName(input.UserName), cancellationToken);
        if (user is null || !user.IsActive || !_passwordHasher.VerifyPassword(input.Password, user.PasswordHash))
        {
            throw new NexusBusinessException(UserErrorCodes.InvalidCredentials, "Invalid user name or password.");
        }

        await _auditWriter.WriteAsync(new AuditLogEntry(Guid.NewGuid(), user.TenantId, user.Id, ServiceName, nameof(User), user.Id.ToString(), AuditAction.Access, "User logged in.", CorrelationContext.CorrelationId, DateTimeOffset.UtcNow), cancellationToken);
        var tokens = await _tokenService.IssueAsync(user, cancellationToken);
        return new LoginResultDto
        {
            UserId = user.Id,
            TenantId = user.TenantId!.Value,
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ExpiresAt = tokens.ExpiresAt
        };
    }

    public async Task<LoginResultDto> RefreshAsync(RefreshTokenDto input, CancellationToken cancellationToken = default)
    {
        var result = await _tokenService.RefreshAsync(input.RefreshToken, cancellationToken)
            ?? throw new NexusBusinessException(UserErrorCodes.InvalidCredentials, "Refresh token is invalid or expired.");

        return new LoginResultDto
        {
            UserId = result.User.Id,
            TenantId = result.User.TenantId!.Value,
            AccessToken = result.Tokens.AccessToken,
            RefreshToken = result.Tokens.RefreshToken,
            ExpiresAt = result.Tokens.ExpiresAt
        };
    }

    public async Task<UserDto> ChangeRolesAsync(Guid id, ChangeUserRolesDto input, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetAsync(id, cancellationToken);
        user.SetRoles(input.Roles, CurrentUser.Id, DateTimeOffset.UtcNow);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _auditWriter.WriteAsync(new AuditLogEntry(Guid.NewGuid(), user.TenantId, CurrentUser.Id, ServiceName, nameof(User), user.Id.ToString(), AuditAction.Update, "User roles changed.", CorrelationContext.CorrelationId, DateTimeOffset.UtcNow), cancellationToken);
        await _eventBus.PublishAsync(new UserRoleChangedIntegrationEvent(Guid.NewGuid(), user.TenantId, DateTimeOffset.UtcNow, ServiceName, CorrelationContext.CorrelationId, user.Id, user.Roles.Select(x => x.RoleName).ToArray()), cancellationToken);
        return MapToDto(user);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            TenantId = user.TenantId!.Value,
            UserName = user.UserName,
            Email = user.Email,
            IsActive = user.IsActive,
            Roles = user.Roles.Select(x => x.RoleName).ToArray(),
            ConcurrencyStamp = user.ConcurrencyStamp
        };
    }
}
