using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.Repositories;
using Nexus.Services.Crm.Domain.Enums;
using Nexus.Services.Crm.Domain.Opportunities;

namespace Nexus.Services.Crm.Infrastructure.Repositories;

public sealed class EfCoreOpportunityRepository : EfCoreRepository<Opportunity, Guid>, IOpportunityRepository
{
    public EfCoreOpportunityRepository(NexusDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Opportunity>> GetListByTenantAsync(
        Guid tenantId,
        string? search,
        string? stage,
        Guid? customerId,
        Guid? ownerId,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(tenantId, search, stage, customerId, ownerId)
            .OrderByDescending(x => x.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountByTenantAsync(
        Guid tenantId,
        string? search,
        string? stage,
        Guid? customerId,
        Guid? ownerId,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(tenantId, search, stage, customerId, ownerId).LongCountAsync(cancellationToken);
    }

    public async Task<decimal> GetOpenPipelineValueAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await Set
            .Where(x => x.TenantId == tenantId
                && x.Stage != OpportunityStage.ClosedWon
                && x.Stage != OpportunityStage.ClosedLost)
            .SumAsync(x => x.Amount, cancellationToken);
    }

    private IQueryable<Opportunity> ApplyFilters(
        Guid tenantId,
        string? search,
        string? stage,
        Guid? customerId,
        Guid? ownerId)
    {
        var query = Set.Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(stage)
            && Enum.TryParse<OpportunityStage>(stage, true, out var stageEnum))
        {
            query = query.Where(x => x.Stage == stageEnum);
        }

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId);
        }

        if (ownerId.HasValue)
        {
            query = query.Where(x => x.OwnerId == ownerId);
        }

        return query;
    }
}
