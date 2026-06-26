namespace Nexus.Services.Tenant.Contracts.Subscriptions;

public sealed class SubscriptionPlanDto
{
    public string PlanCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal MonthlyPrice { get; init; }
    public IReadOnlyList<string> Modules { get; init; } = [];
    public int MaxUsers { get; init; }
    public int StorageGb { get; init; }
    public int TierOrder { get; init; }
}

public sealed class CreateCheckoutDto
{
    public required string TargetPlanCode { get; init; }
}

public sealed class CheckoutSessionDto
{
    public Guid CheckoutId { get; init; }
    public string TargetPlanCode { get; init; } = string.Empty;
    public string TargetPlanName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string MockCardNumber { get; init; } = "4242 4242 4242 4242";
}

public sealed class SubscriptionPaymentDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string PlanCode { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? MockReference { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? PaidAt { get; init; }
}

public sealed class PlatformDashboardDto
{
    public int TotalTenants { get; init; }
    public int ActiveTenants { get; init; }
    public int SuspendedTenants { get; init; }
    public int NewTenantsLast7Days { get; init; }
    public int NewTenantsLast30Days { get; init; }
    public int NewUsersLast7Days { get; init; }
    public int NewUsersLast30Days { get; init; }
    public int ActiveSubscriptions { get; init; }
    public decimal MonthlyRecurringRevenue { get; init; }
    public IReadOnlyDictionary<string, int> ActiveSubscriptionsByPlan { get; init; } = new Dictionary<string, int>();
    public IReadOnlyList<PlatformTimeSeriesPointDto> TenantGrowthSeries { get; init; } = [];
    public IReadOnlyList<PlatformTimeSeriesPointDto> UserGrowthSeries { get; init; } = [];
    public IReadOnlyList<PlatformRevenuePointDto> RevenueLast6Months { get; init; } = [];
    public IReadOnlyList<PlatformRecentTenantDto> RecentTenants { get; init; } = [];
}

public sealed class PlatformTimeSeriesPointDto
{
    public DateOnly Date { get; init; }
    public int Count { get; init; }
}

public sealed class PlatformRevenuePointDto
{
    public string Month { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}

public sealed class PlatformRecentTenantDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? PlanCode { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class PlatformUserStatsDto
{
    public int NewUsersLast7Days { get; init; }
    public int NewUsersLast30Days { get; init; }
    public IReadOnlyList<PlatformTimeSeriesPointDto> UserGrowthSeries { get; init; } = [];
}
