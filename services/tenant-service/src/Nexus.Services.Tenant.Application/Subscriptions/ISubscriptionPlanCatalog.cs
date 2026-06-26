namespace Nexus.Services.Tenant.Application.Subscriptions;

public interface ISubscriptionPlanCatalog
{
    IReadOnlyList<SubscriptionPlanDefinition> GetAll();
    SubscriptionPlanDefinition GetRequired(string planCode);
    bool IsUpgradeAllowed(string currentPlanCode, string targetPlanCode);
}
