using Nexus.ApiContracts.Dtos;

namespace Nexus.Services.Tenant.Contracts.Tenants;

public interface ITenantAppService
{
    Task<PagedResultDto<TenantDto>> GetListAsync(GetTenantsInput input, CancellationToken cancellationToken = default);
    Task<TenantDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TenantDto> CreateAsync(CreateTenantDto input, CancellationToken cancellationToken = default);
    Task<TenantDto> UpdateSettingsAsync(Guid id, UpdateTenantSettingsDto input, CancellationToken cancellationToken = default);
    Task<TenantDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TenantDto> SuspendAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TenantDto> EnableModuleAsync(Guid id, ChangeTenantModuleDto input, CancellationToken cancellationToken = default);
    Task<TenantDto> DisableModuleAsync(Guid id, ChangeTenantModuleDto input, CancellationToken cancellationToken = default);
}
