using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.Repositories;
using Nexus.Services.Crm.Domain.Contacts;

namespace Nexus.Services.Crm.Infrastructure.Repositories;

public sealed class EfCoreContactRepository : EfCoreRepository<Contact, Guid>, IContactRepository
{
    public EfCoreContactRepository(NexusDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Contact>> GetListByTenantAsync(
        Guid tenantId,
        Guid? customerId,
        string? search,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(tenantId, customerId, search)
            .OrderBy(x => x.FullName)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountByTenantAsync(
        Guid tenantId,
        Guid? customerId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(tenantId, customerId, search).LongCountAsync(cancellationToken);
    }

    private IQueryable<Contact> ApplyFilters(Guid tenantId, Guid? customerId, string? search)
    {
        var query = Set.Where(x => x.TenantId == tenantId);

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.FullName.ToLower().Contains(term)
                || (x.Email != null && x.Email.ToLower().Contains(term))
                || (x.Phone != null && x.Phone.ToLower().Contains(term)));
        }

        return query;
    }
}
