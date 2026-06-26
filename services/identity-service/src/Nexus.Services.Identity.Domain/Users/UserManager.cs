using Nexus.SharedKernel.Context;
using Nexus.SharedKernel.Exceptions;

namespace Nexus.Services.Identity.Domain.Users;

public sealed class UserManager
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUser _currentUser;

    public UserManager(IUserRepository userRepository, IPasswordHasher passwordHasher, ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<User> CreateAsync(Guid tenantId, string userName, string email, string password, IReadOnlyCollection<string> roles, CancellationToken cancellationToken = default)
    {
        var normalizedUserName = User.NormalizeUserName(userName);
        var normalizedEmail = User.NormalizeEmail(email);

        if (await _userRepository.FindByUserNameAsync(tenantId, normalizedUserName, cancellationToken) is not null ||
            await _userRepository.FindByEmailAsync(tenantId, normalizedEmail, cancellationToken) is not null)
        {
            throw new NexusBusinessException(UserErrorCodes.AlreadyExists, "User name or email already exists in this tenant.");
        }

        var user = new User(Guid.NewGuid(), tenantId, normalizedUserName, normalizedEmail, _passwordHasher.HashPassword(password), _currentUser.Id, DateTimeOffset.UtcNow);
        user.SetRoles(roles, _currentUser.Id, DateTimeOffset.UtcNow);
        return await _userRepository.InsertAsync(user, cancellationToken);
    }
}
