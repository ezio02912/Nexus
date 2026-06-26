using Nexus.Services.Tenant.Contracts.Subscriptions;
using Nexus.Services.Tenant.Contracts.Tenants;

namespace Nexus.Services.Tenant.Contracts.Subscriptions;

public interface ISubscriptionAppService
{
    IReadOnlyList<SubscriptionPlanDto> GetPlans();
    Task<TenantSubscriptionDto?> GetTenantSubscriptionAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface IBillingAppService
{
    Task<CheckoutSessionDto> CreateCheckoutAsync(CreateCheckoutDto input, CancellationToken cancellationToken = default);
    Task<TenantSubscriptionDto> ConfirmCheckoutAsync(Guid checkoutId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubscriptionPaymentDto>> GetInvoicesAsync(CancellationToken cancellationToken = default);
}

public interface IPlatformDashboardAppService
{
    Task<PlatformDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
