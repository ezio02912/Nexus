using Nexus.Services.Tenant.Contracts.Subscriptions;

namespace Nexus.Services.Tenant.Contracts.Platform;

public interface IPlatformUserStatsProvider
{
    Task<PlatformUserStatsDto> GetUserStatsAsync(CancellationToken cancellationToken = default);
}
