using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.Repositories;
using Nexus.Services.Crm.Domain.Enums;
using Nexus.Services.Crm.Domain.Leads;

namespace Nexus.Services.Crm.Infrastructure.Repositories;

public sealed class EfCoreLeadRepository : EfCoreRepository<Lead, Guid>, ILeadRepository
{
    public EfCoreLeadRepository(NexusDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Lead>> GetListByTenantAsync(
        Guid tenantId,
        string? search,
        string? status,
        Guid? ownerId,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(tenantId, search, status, ownerId)
            .OrderByDescending(x => x.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountByTenantAsync(
        Guid tenantId,
        string? search,
        string? status,
        Guid? ownerId,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(tenantId, search, status, ownerId).LongCountAsync(cancellationToken);
    }

    private IQueryable<Lead> ApplyFilters(Guid tenantId, string? search, string? status, Guid? ownerId)
    {
        var query = Set.Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.FullName.ToLower().Contains(term)
                || (x.CompanyName != null && x.CompanyName.ToLower().Contains(term))
                || (x.Email != null && x.Email.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<LeadStatus>(status, true, out var statusEnum))
        {
            query = query.Where(x => x.Status == statusEnum);
        }

        if (ownerId.HasValue)
        {
            query = query.Where(x => x.OwnerId == ownerId);
        }

        return query;
    }
}
