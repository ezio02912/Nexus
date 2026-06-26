using Nexus.SharedKernel.Repositories;

namespace Nexus.Services.Tenant.Domain.Billing;

public interface ISubscriptionPaymentRepository : IRepository<SubscriptionPayment, Guid>
{
    Task<SubscriptionPayment?> FindPendingAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubscriptionPayment>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubscriptionPayment>> GetPaidSinceAsync(DateTimeOffset since, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubscriptionPayment>> GetRecentAsync(int maxCount, CancellationToken cancellationToken = default);
}
