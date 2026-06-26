namespace Nexus.Services.Tenant.Domain.Tenants;

public sealed record RecentTenantSnapshot(
    Guid Id,
    string Code,
    string Name,
    string? PlanCode,
    DateTimeOffset CreatedAt);

public sealed record DailyCountSnapshot(DateOnly Date, int Count);
