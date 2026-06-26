using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.Repositories;
using Nexus.Services.Tenant.Domain.Tenants;
using TenantAggregate = Nexus.Services.Tenant.Domain.Tenants.Tenant;

namespace Nexus.Services.Tenant.Infrastructure.Tenants;

public sealed class EfCoreTenantRepository : EfCoreRepository<TenantAggregate, Guid>, ITenantRepository
{
    public EfCoreTenantRepository(NexusDbContext context) : base(context)
    {
    }

    public override async Task<TenantAggregate?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public override async Task<IReadOnlyList<TenantAggregate>> GetListAsync(int skipCount = 0, int maxResultCount = 50, string? sorting = null, CancellationToken cancellationToken = default)
    {
        return await Query()
            .OrderBy(x => x.Code)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<TenantAggregate?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = TenantAggregate.NormalizeCode(code);
        return await Query().SingleOrDefaultAsync(x => x.Code == normalized, cancellationToken);
    }

    private IQueryable<TenantAggregate> Query() =>
        Set.Include(x => x.Modules).Include(x => x.Settings).Include(x => x.Subscription);
}
