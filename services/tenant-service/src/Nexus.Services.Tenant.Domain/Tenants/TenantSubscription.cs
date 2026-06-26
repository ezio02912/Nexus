using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Validation;

namespace Nexus.Services.Tenant.Domain.Tenants;

public sealed class TenantSubscription : NexusEntity<Guid>
{
    private TenantSubscription()
    {
        PlanCode = string.Empty;
    }

    public TenantSubscription(Guid id, Guid tenantId, string planCode, DateTimeOffset? expiresAt)
    {
        Id = id;
        TenantId = tenantId;
        PlanCode = Check.Length(Check.NotNullOrWhiteSpace(planCode, nameof(planCode)), nameof(planCode), TenantConsts.PlanCodeMaxLength);
        ExpiresAt = expiresAt;
    }

    public Guid TenantId { get; private set; }
    public string PlanCode { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    public void ChangePlan(string planCode, DateTimeOffset? expiresAt)
    {
        PlanCode = Check.Length(Check.NotNullOrWhiteSpace(planCode, nameof(planCode)), nameof(planCode), TenantConsts.PlanCodeMaxLength);
        ExpiresAt = expiresAt;
    }
}
