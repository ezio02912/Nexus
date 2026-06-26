using Nexus.Services.Tenant.Application.Subscriptions;
using Nexus.Services.Tenant.Contracts.Platform;
using Nexus.Services.Tenant.Contracts.Subscriptions;
using Nexus.Services.Tenant.Domain.Billing;
using Nexus.Services.Tenant.Domain.Tenants;

namespace Nexus.Services.Tenant.Application.Platform;

public sealed class PlatformDashboardAppService : IPlatformDashboardAppService
{
    private readonly ITenantDashboardRepository _dashboardRepository;
    private readonly ISubscriptionPlanCatalog _planCatalog;
    private readonly ISubscriptionPaymentRepository _paymentRepository;
    private readonly IPlatformUserStatsProvider _userStatsProvider;

    public PlatformDashboardAppService(
        ITenantDashboardRepository dashboardRepository,
        ISubscriptionPlanCatalog planCatalog,
        ISubscriptionPaymentRepository paymentRepository,
        IPlatformUserStatsProvider userStatsProvider)
    {
        _dashboardRepository = dashboardRepository;
        _planCatalog = planCatalog;
        _paymentRepository = paymentRepository;
        _userStatsProvider = userStatsProvider;
    }

    public async Task<PlatformDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var since7 = now.AddDays(-7);
        var since30 = now.AddDays(-30);
        var since6Months = now.AddMonths(-6);

        var userStats = await _userStatsProvider.GetUserStatsAsync(cancellationToken);
        var subscriptionsByPlan = await _dashboardRepository.GetActiveSubscriptionCountsByPlanAsync(cancellationToken);
        var tenantSeries = await _dashboardRepository.GetDailyCreationCountsAsync(since30, cancellationToken);
        var recentTenants = await _dashboardRepository.GetRecentTenantsAsync(8, cancellationToken);
        var paidPayments = await _paymentRepository.GetPaidSinceAsync(since6Months, cancellationToken);

        var mrr = subscriptionsByPlan.Sum(x =>
        {
            try
            {
                return _planCatalog.GetRequired(x.Key).MonthlyPrice * x.Value;
            }
            catch
            {
                return 0m;
            }
        });

        return new PlatformDashboardDto
        {
            TotalTenants = await _dashboardRepository.GetCountAsync(cancellationToken),
            ActiveTenants = await _dashboardRepository.GetCountByStatusAsync(TenantStatus.Active, cancellationToken),
            SuspendedTenants = await _dashboardRepository.GetCountByStatusAsync(TenantStatus.Suspended, cancellationToken),
            NewTenantsLast7Days = await _dashboardRepository.GetCountCreatedSinceAsync(since7, cancellationToken),
            NewTenantsLast30Days = await _dashboardRepository.GetCountCreatedSinceAsync(since30, cancellationToken),
            NewUsersLast7Days = userStats.NewUsersLast7Days,
            NewUsersLast30Days = userStats.NewUsersLast30Days,
            ActiveSubscriptions = subscriptionsByPlan.Values.Sum(),
            MonthlyRecurringRevenue = mrr,
            ActiveSubscriptionsByPlan = subscriptionsByPlan,
            TenantGrowthSeries = tenantSeries.Select(x => new PlatformTimeSeriesPointDto { Date = x.Date, Count = x.Count }).ToArray(),
            UserGrowthSeries = userStats.UserGrowthSeries,
            RevenueLast6Months = BuildRevenueSeries(paidPayments, since6Months),
            RecentTenants = recentTenants.Select(x => new PlatformRecentTenantDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                PlanCode = x.PlanCode,
                CreatedAt = x.CreatedAt
            }).ToArray()
        };
    }

    private static IReadOnlyList<PlatformRevenuePointDto> BuildRevenueSeries(
        IReadOnlyList<SubscriptionPayment> payments,
        DateTimeOffset since)
    {
        var start = new DateOnly(since.Year, since.Month, 1);
        var end = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var buckets = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var payment in payments.Where(x => x.PaidAt is not null))
        {
            var paidAt = payment.PaidAt!.Value;
            var key = $"{paidAt.Year:0000}-{paidAt.Month:00}";
            buckets[key] = buckets.GetValueOrDefault(key) + payment.Amount;
        }

        var results = new List<PlatformRevenuePointDto>();
        for (var cursor = start; cursor <= end; cursor = cursor.AddMonths(1))
        {
            var key = $"{cursor.Year:0000}-{cursor.Month:00}";
            results.Add(new PlatformRevenuePointDto
            {
                Month = key,
                Amount = buckets.GetValueOrDefault(key)
            });
        }

        return results;
    }
}
