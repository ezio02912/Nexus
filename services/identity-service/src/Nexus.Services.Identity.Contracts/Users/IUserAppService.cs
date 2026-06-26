using Nexus.ApiContracts.Dtos;

namespace Nexus.Services.Identity.Contracts.Users;

public interface IUserAppService
{
    Task<PagedResultDto<UserDto>> GetListAsync(GetUsersInput input, CancellationToken cancellationToken = default);
    Task<UserDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserDto input, CancellationToken cancellationToken = default);
    Task<LoginResultDto> LoginAsync(LoginDto input, CancellationToken cancellationToken = default);
    Task<LoginResultDto> RefreshAsync(RefreshTokenDto input, CancellationToken cancellationToken = default);
    Task<UserDto> ChangeRolesAsync(Guid id, ChangeUserRolesDto input, CancellationToken cancellationToken = default);
}
