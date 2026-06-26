using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.Repositories;
using Nexus.Services.Identity.Domain.Users;

namespace Nexus.Services.Identity.Infrastructure.Users;

public sealed class EfCoreUserRepository : EfCoreRepository<User, Guid>, IUserRepository
{
    public EfCoreUserRepository(NexusDbContext context) : base(context)
    {
    }

    public override async Task<User?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Set.Include(x => x.Roles).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public override async Task<IReadOnlyList<User>> GetListAsync(int skipCount = 0, int maxResultCount = 50, string? sorting = null, CancellationToken cancellationToken = default)
    {
        return await Set
            .Include(x => x.Roles)
            .OrderBy(x => x.UserName)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> FindByUserNameAsync(Guid tenantId, string userName, CancellationToken cancellationToken = default)
    {
        var normalized = User.NormalizeUserName(userName);
        return await Set
            .Include(x => x.Roles)
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.UserName == normalized, cancellationToken);
    }

    public async Task<User?> FindByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        var normalized = User.NormalizeEmail(email);
        return await Set
            .Include(x => x.Roles)
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Email == normalized, cancellationToken);
    }

    public async Task<User?> FindByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Set.Include(x => x.Roles).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
