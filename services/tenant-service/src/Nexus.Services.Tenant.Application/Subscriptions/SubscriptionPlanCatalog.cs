using Microsoft.Extensions.Configuration;
using Nexus.SharedKernel.Exceptions;

namespace Nexus.Services.Tenant.Application.Subscriptions;

public sealed class SubscriptionPlanCatalog : ISubscriptionPlanCatalog
{
    private readonly IReadOnlyDictionary<string, SubscriptionPlanDefinition> _plans;

    public SubscriptionPlanCatalog(IConfiguration configuration)
    {
        var section = configuration.GetSection("SubscriptionPlans");
        var plans = new Dictionary<string, SubscriptionPlanDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var child in section.GetChildren())
        {
            var planCode = child.Key.ToUpperInvariant();
            var modules = child.GetSection("Modules").Get<string[]>() ?? [];
            plans[planCode] = new SubscriptionPlanDefinition
            {
                PlanCode = planCode,
                Name = child["Name"] ?? planCode,
                MonthlyPrice = child.GetValue<decimal>("MonthlyPrice"),
                Modules = modules.Select(x => x.Trim().ToUpperInvariant()).Distinct().ToArray(),
                MaxUsers = child.GetValue("MaxUsers", 0),
                StorageGb = child.GetValue("StorageGb", 0),
                TierOrder = child.GetValue("TierOrder", 0)
            };
        }

        _plans = plans;
    }

    public IReadOnlyList<SubscriptionPlanDefinition> GetAll()
        => _plans.Values.OrderBy(x => x.TierOrder).ToArray();

    public SubscriptionPlanDefinition GetRequired(string planCode)
    {
        var normalized = planCode.Trim().ToUpperInvariant();
        if (_plans.TryGetValue(normalized, out var plan))
        {
            return plan;
        }

        throw new NexusBusinessException("Subscription.PlanNotFound", $"Subscription plan '{planCode}' was not found.");
    }

    public bool IsUpgradeAllowed(string currentPlanCode, string targetPlanCode)
    {
        var current = GetRequired(currentPlanCode);
        var target = GetRequired(targetPlanCode);
        return target.TierOrder > current.TierOrder;
    }
}
