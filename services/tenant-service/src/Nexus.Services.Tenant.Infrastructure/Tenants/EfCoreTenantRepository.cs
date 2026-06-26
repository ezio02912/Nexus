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

    public override async Task<TenantAggregate> UpdateAsync(TenantAggregate entity, CancellationToken cancellationToken = default)
    {
        Context.ChangeTracker.DetectChanges();

        foreach (var module in entity.Modules)
        {
            var entry = Context.Entry(module);
            if (entry.State == EntityState.Detached)
            {
                Context.Set<TenantModule>().Add(module);
                entry = Context.Entry(module);
            }

            // Client-generated keys make EF assume the row already exists (Modified).
            if (entry.State == EntityState.Modified
                && !await Context.Set<TenantModule>().AnyAsync(x => x.Id == module.Id, cancellationToken))
            {
                entry.State = EntityState.Added;
            }
        }

        foreach (var setting in entity.Settings)
        {
            var entry = Context.Entry(setting);
            if (entry.State == EntityState.Detached)
            {
                Context.Set<TenantSetting>().Add(setting);
                entry = Context.Entry(setting);
            }

            if (entry.State == EntityState.Modified
                && !await Context.Set<TenantSetting>().AnyAsync(x => x.Id == setting.Id, cancellationToken))
            {
                entry.State = EntityState.Added;
            }
        }

        await Context.SaveChangesAsync(cancellationToken);
        return entity;
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
