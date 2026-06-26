namespace Nexus.Services.Tenant.Application.Subscriptions;

public sealed class SubscriptionPlanDefinition
{
    public required string PlanCode { get; init; }
    public required string Name { get; init; }
    public decimal MonthlyPrice { get; init; }
    public IReadOnlyList<string> Modules { get; init; } = [];
    public int MaxUsers { get; init; }
    public int StorageGb { get; init; }
    public int TierOrder { get; init; }
}
