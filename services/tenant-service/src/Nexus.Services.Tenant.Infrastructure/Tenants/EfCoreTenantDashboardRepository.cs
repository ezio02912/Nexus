using Microsoft.EntityFrameworkCore;
using Nexus.Services.Tenant.Domain.Tenants;
using Nexus.Services.Tenant.Infrastructure.Persistence;
using TenantAggregate = Nexus.Services.Tenant.Domain.Tenants.Tenant;

namespace Nexus.Services.Tenant.Infrastructure.Tenants;

public sealed class EfCoreTenantDashboardRepository : ITenantDashboardRepository
{
    private readonly TenantDbContext _db;

    public EfCoreTenantDashboardRepository(TenantDbContext db)
    {
        _db = db;
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        => _db.Tenants.IgnoreQueryFilters().CountAsync(cancellationToken);

    public Task<int> GetCountByStatusAsync(TenantStatus status, CancellationToken cancellationToken = default)
        => _db.Tenants.IgnoreQueryFilters().CountAsync(x => x.Status == status, cancellationToken);

    public Task<int> GetCountCreatedSinceAsync(DateTimeOffset since, CancellationToken cancellationToken = default)
        => _db.Tenants.IgnoreQueryFilters().CountAsync(x => x.CreationTime >= since, cancellationToken);

    public async Task<IReadOnlyList<DailyCountSnapshot>> GetDailyCreationCountsAsync(DateTimeOffset since, CancellationToken cancellationToken = default)
    {
        var tenants = await _db.Tenants
            .IgnoreQueryFilters()
            .Where(x => x.CreationTime >= since)
            .Select(x => x.CreationTime)
            .ToListAsync(cancellationToken);

        return tenants
            .GroupBy(x => DateOnly.FromDateTime(x.UtcDateTime))
            .OrderBy(x => x.Key)
            .Select(x => new DailyCountSnapshot(x.Key, x.Count()))
            .ToArray();
    }

    public async Task<IReadOnlyDictionary<string, int>> GetActiveSubscriptionCountsByPlanAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _db.Tenants
            .IgnoreQueryFilters()
            .Include(x => x.Subscription)
            .Where(x => x.Status == TenantStatus.Active && x.Subscription != null)
            .Select(x => x.Subscription!.PlanCode)
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Count(), StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<RecentTenantSnapshot>> GetRecentTenantsAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        return await _db.Tenants
            .IgnoreQueryFilters()
            .Include(x => x.Subscription)
            .OrderByDescending(x => x.CreationTime)
            .Take(maxCount)
            .Select(x => new RecentTenantSnapshot(
                x.Id,
                x.Code,
                x.Name,
                x.Subscription != null ? x.Subscription.PlanCode : null,
                x.CreationTime))
            .ToListAsync(cancellationToken);
    }
}
