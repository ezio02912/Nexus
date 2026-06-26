using Nexus.BuildingBlocks.Application;
using Nexus.EventContracts.Tenants;
using Nexus.Services.Tenant.Application.Subscriptions;
using Nexus.Services.Tenant.Contracts.Subscriptions;
using Nexus.Services.Tenant.Contracts.Tenants;
using Nexus.Services.Tenant.Domain.Billing;
using Nexus.Services.Tenant.Domain.Tenants;
using Nexus.SharedKernel.Context;
using Nexus.SharedKernel.Events;
using Nexus.SharedKernel.Exceptions;

namespace Nexus.Services.Tenant.Application.Billing;

public sealed class BillingAppService : NexusAppServiceBase, IBillingAppService
{
    private const string ServiceName = "tenant-service";
    private readonly ITenantRepository _tenantRepository;
    private readonly ISubscriptionPaymentRepository _paymentRepository;
    private readonly ISubscriptionPlanCatalog _planCatalog;
    private readonly SubscriptionAppService _subscriptionAppService;
    private readonly IEventBus _eventBus;

    public BillingAppService(
        ITenantRepository tenantRepository,
        ISubscriptionPaymentRepository paymentRepository,
        ISubscriptionPlanCatalog planCatalog,
        SubscriptionAppService subscriptionAppService,
        IEventBus eventBus,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        ICorrelationContext correlationContext) : base(currentTenant, currentUser, correlationContext)
    {
        _tenantRepository = tenantRepository;
        _paymentRepository = paymentRepository;
        _planCatalog = planCatalog;
        _subscriptionAppService = subscriptionAppService;
        _eventBus = eventBus;
    }

    public async Task<CheckoutSessionDto> CreateCheckoutAsync(CreateCheckoutDto input, CancellationToken cancellationToken = default)
    {
        EnsureTenantContext();

        var tenant = await _tenantRepository.GetAsync(CurrentTenant.Id!.Value, cancellationToken);
        var currentPlanCode = tenant.Subscription?.PlanCode ?? "FREE";
        var targetPlan = _planCatalog.GetRequired(input.TargetPlanCode);

        if (!_planCatalog.IsUpgradeAllowed(currentPlanCode, targetPlan.PlanCode))
        {
            throw new NexusBusinessException("Billing.UpgradeNotAllowed", "Only upgrades to a higher plan are allowed.");
        }

        if (targetPlan.MonthlyPrice <= 0)
        {
            throw new NexusBusinessException("Billing.InvalidPlan", "Target plan does not require payment.");
        }

        var payment = new SubscriptionPayment(
            Guid.NewGuid(),
            tenant.Id,
            targetPlan.PlanCode,
            targetPlan.MonthlyPrice,
            $"MOCK-{Guid.NewGuid():N}"[..16].ToUpperInvariant(),
            DateTimeOffset.UtcNow);

        await _paymentRepository.InsertAsync(payment, cancellationToken);

        return new CheckoutSessionDto
        {
            CheckoutId = payment.Id,
            TargetPlanCode = targetPlan.PlanCode,
            TargetPlanName = targetPlan.Name,
            Amount = targetPlan.MonthlyPrice
        };
    }

    public async Task<TenantSubscriptionDto> ConfirmCheckoutAsync(Guid checkoutId, CancellationToken cancellationToken = default)
    {
        EnsureTenantContext();

        var tenantId = CurrentTenant.Id!.Value;
        var payment = await _paymentRepository.FindPendingAsync(checkoutId, tenantId, cancellationToken)
            ?? throw new NexusBusinessException("Billing.CheckoutNotFound", "Checkout session was not found.");

        var tenant = await _tenantRepository.GetAsync(tenantId, cancellationToken);
        var oldPlanCode = tenant.Subscription?.PlanCode ?? "FREE";
        var targetPlan = _planCatalog.GetRequired(payment.PlanCode);
        var now = DateTimeOffset.UtcNow;

        payment.MarkPaid(now);
        tenant.SetSubscription(targetPlan.PlanCode, now.AddDays(30), CurrentUser.Id, now);
        SubscriptionPlanApplier.ApplyModules(tenant, targetPlan, CurrentUser.Id, now);

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);

        await _eventBus.PublishAsync(new SubscriptionChangedIntegrationEvent(
            Guid.NewGuid(),
            tenant.Id,
            now,
            ServiceName,
            CorrelationContext.CorrelationId,
            tenant.Id,
            oldPlanCode,
            targetPlan.PlanCode,
            payment.Amount), cancellationToken);

        return _subscriptionAppService.MapSubscription(tenant.Subscription!);
    }

    public async Task<IReadOnlyList<SubscriptionPaymentDto>> GetInvoicesAsync(CancellationToken cancellationToken = default)
    {
        EnsureTenantContext();
        var payments = await _paymentRepository.GetByTenantAsync(CurrentTenant.Id!.Value, cancellationToken);
        return payments.Select(MapPayment).ToArray();
    }

    private void EnsureTenantContext()
    {
        if (CurrentTenant.Id is null)
        {
            throw new NexusBusinessException("Billing.TenantRequired", "Tenant context is required.");
        }
    }

    private static SubscriptionPaymentDto MapPayment(SubscriptionPayment payment) => new()
    {
        Id = payment.Id,
        TenantId = payment.TenantId,
        PlanCode = payment.PlanCode,
        Amount = payment.Amount,
        Status = payment.Status,
        MockReference = payment.MockReference,
        CreatedAt = payment.CreatedAt,
        PaidAt = payment.PaidAt
    };
}
