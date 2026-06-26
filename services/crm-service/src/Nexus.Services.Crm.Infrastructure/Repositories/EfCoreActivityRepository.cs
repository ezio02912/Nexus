using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.Repositories;
using Nexus.Services.Crm.Domain.Activities;
using Nexus.Services.Crm.Domain.Enums;

namespace Nexus.Services.Crm.Infrastructure.Repositories;

public sealed class EfCoreActivityRepository : EfCoreRepository<CrmActivity, Guid>, IActivityRepository
{
    public EfCoreActivityRepository(NexusDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<CrmActivity>> GetListByTenantAsync(
        Guid tenantId,
        CrmRelatedEntityType? relatedEntityType,
        Guid? relatedEntityId,
        string? status,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(tenantId, relatedEntityType, relatedEntityId, status)
            .OrderByDescending(x => x.ActivityDate)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountByTenantAsync(
        Guid tenantId,
        CrmRelatedEntityType? relatedEntityType,
        Guid? relatedEntityId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(tenantId, relatedEntityType, relatedEntityId, status).LongCountAsync(cancellationToken);
    }

    private IQueryable<CrmActivity> ApplyFilters(
        Guid tenantId,
        CrmRelatedEntityType? relatedEntityType,
        Guid? relatedEntityId,
        string? status)
    {
        var query = Set.Where(x => x.TenantId == tenantId);

        if (relatedEntityType.HasValue)
        {
            query = query.Where(x => x.RelatedEntityType == relatedEntityType);
        }

        if (relatedEntityId.HasValue)
        {
            query = query.Where(x => x.RelatedEntityId == relatedEntityId);
        }

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<CrmActivityStatus>(status, true, out var statusEnum))
        {
            query = query.Where(x => x.Status == statusEnum);
        }

        return query;
    }
}
