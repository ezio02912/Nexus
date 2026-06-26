using Nexus.SharedKernel.Repositories;

namespace Nexus.Services.Tenant.Domain.Tenants;

public interface ITenantRepository : IRepository<Tenant, Guid>
{
    Task<Tenant?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}
