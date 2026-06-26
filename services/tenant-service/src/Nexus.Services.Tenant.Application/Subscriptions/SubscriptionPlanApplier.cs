using Nexus.Services.Tenant.Domain.Tenants;
using TenantAggregate = Nexus.Services.Tenant.Domain.Tenants.Tenant;

namespace Nexus.Services.Tenant.Application.Subscriptions;

public static class SubscriptionPlanApplier
{
    public static void ApplyModules(
        TenantAggregate tenant,
        SubscriptionPlanDefinition plan,
        Guid? modifierId,
        DateTimeOffset now)
    {
        var planModules = plan.Modules
            .Select(TenantAggregate.NormalizeCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var module in planModules)
        {
            tenant.EnableModule(module, modifierId, now);
        }

        foreach (var enabled in tenant.Modules.Where(x => x.IsEnabled))
        {
            if (!planModules.Contains(enabled.ModuleCode))
            {
                tenant.DisableModule(enabled.ModuleCode, modifierId, now);
            }
        }
    }
}
