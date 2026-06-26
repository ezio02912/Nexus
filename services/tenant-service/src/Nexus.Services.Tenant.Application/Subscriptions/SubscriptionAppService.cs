using Nexus.Services.Tenant.Application.Subscriptions;
using Nexus.Services.Tenant.Contracts.Subscriptions;
using Nexus.Services.Tenant.Contracts.Tenants;
using Nexus.Services.Tenant.Domain.Tenants;

namespace Nexus.Services.Tenant.Application.Subscriptions;

public sealed class SubscriptionAppService : ISubscriptionAppService
{
    private readonly ISubscriptionPlanCatalog _planCatalog;
    private readonly ITenantRepository _tenantRepository;

    public SubscriptionAppService(ISubscriptionPlanCatalog planCatalog, ITenantRepository tenantRepository)
    {
        _planCatalog = planCatalog;
        _tenantRepository = tenantRepository;
    }

    public IReadOnlyList<SubscriptionPlanDto> GetPlans()
        => _planCatalog.GetAll().Select(MapPlan).ToArray();

    public async Task<TenantSubscriptionDto?> GetTenantSubscriptionAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(tenantId, cancellationToken);
        return tenant.Subscription is null ? null : MapSubscription(tenant.Subscription);
    }

    internal TenantSubscriptionDto MapSubscription(TenantSubscription subscription)
    {
        var plan = _planCatalog.GetRequired(subscription.PlanCode);
        return new TenantSubscriptionDto
        {
            PlanCode = plan.PlanCode,
            PlanName = plan.Name,
            MonthlyPrice = plan.MonthlyPrice,
            ExpiresAt = subscription.ExpiresAt
        };
    }

    internal static SubscriptionPlanDto MapPlan(SubscriptionPlanDefinition plan) => new()
    {
        PlanCode = plan.PlanCode,
        Name = plan.Name,
        MonthlyPrice = plan.MonthlyPrice,
        Modules = plan.Modules,
        MaxUsers = plan.MaxUsers,
        StorageGb = plan.StorageGb,
        TierOrder = plan.TierOrder
    };
}
