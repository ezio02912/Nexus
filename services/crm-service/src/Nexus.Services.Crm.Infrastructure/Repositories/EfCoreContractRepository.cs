using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.Repositories;
using Nexus.Services.Crm.Domain.Contracts;
using Nexus.Services.Crm.Domain.Enums;

namespace Nexus.Services.Crm.Infrastructure.Repositories;

public sealed class EfCoreContractRepository : EfCoreRepository<Contract, Guid>, IContractRepository
{
    public EfCoreContractRepository(NexusDbContext context) : base(context)
    {
    }

    public override async Task<Contract?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Set.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public override async Task<Contract> UpdateAsync(Contract entity, CancellationToken cancellationToken = default)
    {
        Context.ChangeTracker.DetectChanges();

        foreach (var line in entity.Lines)
        {
            var entry = Context.Entry(line);
            if (entry.State == EntityState.Detached)
            {
                Context.Set<ContractLine>().Add(line);
                entry = Context.Entry(line);
            }

            if (entry.State == EntityState.Modified
                && !await Context.Set<ContractLine>().AnyAsync(x => x.Id == line.Id, cancellationToken))
            {
                entry.State = EntityState.Added;
            }
        }

        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Contract?> FindByNoAsync(Guid tenantId, string contractNo, CancellationToken cancellationToken = default)
    {
        var normalized = contractNo.Trim().ToUpperInvariant();
        return await Set.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.ContractNo == normalized, cancellationToken);
    }

    public async Task<Contract?> GetWithLinesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Set.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Contract>> GetListByTenantAsync(
        Guid tenantId,
        string? search,
        string? status,
        Guid? customerId,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(tenantId, search, status, customerId)
            .OrderByDescending(x => x.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountByTenantAsync(
        Guid tenantId,
        string? search,
        string? status,
        Guid? customerId,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(tenantId, search, status, customerId).LongCountAsync(cancellationToken);
    }

    public async Task<long> GetExpiringCountAsync(Guid tenantId, int withinDays, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cutoff = today.AddDays(withinDays);

        return await Set.LongCountAsync(
            x => x.TenantId == tenantId
                && x.EndDate.HasValue
                && x.EndDate.Value >= today
                && x.EndDate.Value <= cutoff
                && (x.Status == ContractStatus.Active || x.Status == ContractStatus.Signed),
            cancellationToken);
    }

    private IQueryable<Contract> ApplyFilters(Guid tenantId, string? search, string? status, Guid? customerId)
    {
        var query = Set.Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.ContractNo.ToLower().Contains(term)
                || x.Title.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<ContractStatus>(status, true, out var statusEnum))
        {
            query = query.Where(x => x.Status == statusEnum);
        }

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId);
        }

        return query;
    }
}
