namespace Nexus.Services.Tenant.Domain.Tenants;

public interface ITenantDashboardRepository
{
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(TenantStatus status, CancellationToken cancellationToken = default);
    Task<int> GetCountCreatedSinceAsync(DateTimeOffset since, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DailyCountSnapshot>> GetDailyCreationCountsAsync(DateTimeOffset since, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, int>> GetActiveSubscriptionCountsByPlanAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecentTenantSnapshot>> GetRecentTenantsAsync(int maxCount, CancellationToken cancellationToken = default);
}
