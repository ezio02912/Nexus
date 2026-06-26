using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.Repositories;
using Nexus.Services.Crm.Domain.Customers;
using Nexus.Services.Crm.Domain.Enums;

namespace Nexus.Services.Crm.Infrastructure.Repositories;

public sealed class EfCoreCustomerRepository : EfCoreRepository<Customer, Guid>, ICustomerRepository
{
    public EfCoreCustomerRepository(NexusDbContext context) : base(context)
    {
    }

    public async Task<Customer?> FindByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
    {
        var normalized = Customer.NormalizeCode(code);
        return await Set.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Code == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetListByTenantAsync(
        Guid tenantId,
        string? search,
        string? status,
        Guid? ownerId,
        int skipCount,
        int maxResultCount,
        string? sorting,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(tenantId, search, status, ownerId);
        query = ApplySorting(query, sorting);

        return await query
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

    private IQueryable<Customer> ApplyFilters(Guid tenantId, string? search, string? status, Guid? ownerId)
    {
        var query = Set.Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x => x.Code.ToLower().Contains(term) || x.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<CustomerStatus>(status, true, out var statusEnum))
        {
            query = query.Where(x => x.Status == statusEnum);
        }

        if (ownerId.HasValue)
        {
            query = query.Where(x => x.OwnerId == ownerId);
        }

        return query;
    }

    private static IQueryable<Customer> ApplySorting(IQueryable<Customer> query, string? sorting)
    {
        if (string.Equals(sorting, "Code", StringComparison.OrdinalIgnoreCase))
        {
            return query.OrderBy(x => x.Code);
        }

        if (string.Equals(sorting, "CreationTime", StringComparison.OrdinalIgnoreCase))
        {
            return query.OrderByDescending(x => x.CreationTime);
        }

        return query.OrderBy(x => x.Name);
    }
}
