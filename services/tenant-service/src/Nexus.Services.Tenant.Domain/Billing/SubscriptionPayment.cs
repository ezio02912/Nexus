using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Validation;

namespace Nexus.Services.Tenant.Domain.Billing;

public sealed class SubscriptionPayment : NexusEntity<Guid>
{
    public const string StatusPending = "Pending";
    public const string StatusPaid = "Paid";
    public const string StatusFailed = "Failed";

    private SubscriptionPayment()
    {
        PlanCode = string.Empty;
        Status = StatusPending;
    }

    public SubscriptionPayment(
        Guid id,
        Guid tenantId,
        string planCode,
        decimal amount,
        string? mockReference,
        DateTimeOffset createdAt)
    {
        Id = id;
        TenantId = tenantId;
        PlanCode = Check.NotNullOrWhiteSpace(planCode, nameof(planCode)).Trim().ToUpperInvariant();
        Amount = amount;
        Status = StatusPending;
        MockReference = mockReference;
        CreatedAt = createdAt;
    }

    public Guid TenantId { get; private set; }
    public string PlanCode { get; private set; }
    public decimal Amount { get; private set; }
    public string Status { get; private set; }
    public string? MockReference { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }

    public void MarkPaid(DateTimeOffset paidAt)
    {
        Status = StatusPaid;
        PaidAt = paidAt;
    }

    public void MarkFailed()
    {
        Status = StatusFailed;
    }
}
