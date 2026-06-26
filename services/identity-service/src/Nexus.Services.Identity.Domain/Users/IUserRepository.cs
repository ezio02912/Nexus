using Nexus.SharedKernel.Repositories;

namespace Nexus.Services.Identity.Domain.Users;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> FindByUserNameAsync(Guid tenantId, string userName, CancellationToken cancellationToken = default);
    Task<User?> FindByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);
}
