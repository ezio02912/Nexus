using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.Repositories;
using Nexus.Services.Tenant.Domain.Billing;
using Nexus.Services.Tenant.Infrastructure.Persistence;

namespace Nexus.Services.Tenant.Infrastructure.Billing;

public sealed class EfCoreSubscriptionPaymentRepository : EfCoreRepository<SubscriptionPayment, Guid>, ISubscriptionPaymentRepository
{
    public EfCoreSubscriptionPaymentRepository(TenantDbContext context) : base(context)
    {
    }

    public async Task<SubscriptionPayment?> FindPendingAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await Set.SingleOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && x.Status == SubscriptionPayment.StatusPending,
            cancellationToken);
    }

    public async Task<IReadOnlyList<SubscriptionPayment>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await Set
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubscriptionPayment>> GetPaidSinceAsync(DateTimeOffset since, CancellationToken cancellationToken = default)
    {
        return await Set
            .Where(x => x.Status == SubscriptionPayment.StatusPaid && x.PaidAt >= since)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubscriptionPayment>> GetRecentAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        return await Set
            .OrderByDescending(x => x.CreatedAt)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }
}
