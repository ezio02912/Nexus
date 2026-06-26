using Nexus.ApiContracts.Dtos;
using Nexus.BuildingBlocks.Application;
using Nexus.EventContracts.Tenants;
using Nexus.Services.Tenant.Contracts.Tenants;
using Nexus.Services.Tenant.Domain.Tenants;
using Nexus.SharedKernel.Auditing;
using Nexus.SharedKernel.Context;
using Nexus.SharedKernel.Events;
using TenantAggregate = Nexus.Services.Tenant.Domain.Tenants.Tenant;

namespace Nexus.Services.Tenant.Application.Tenants;

public sealed class TenantAppService : NexusAppServiceBase, ITenantAppService
{
    private const string ServiceName = "tenant-service";
    private readonly ITenantRepository _tenantRepository;
    private readonly TenantManager _tenantManager;
    private readonly IEventBus _eventBus;
    private readonly IAuditWriter _auditWriter;

    public TenantAppService(
        ITenantRepository tenantRepository,
        TenantManager tenantManager,
        IEventBus eventBus,
        IAuditWriter auditWriter,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        ICorrelationContext correlationContext) : base(currentTenant, currentUser, correlationContext)
    {
        _tenantRepository = tenantRepository;
        _tenantManager = tenantManager;
        _eventBus = eventBus;
        _auditWriter = auditWriter;
    }

    public async Task<PagedResultDto<TenantDto>> GetListAsync(GetTenantsInput input, CancellationToken cancellationToken = default)
    {
        var items = await _tenantRepository.GetListAsync(input.SkipCount, input.MaxResultCount, input.Sorting, cancellationToken);
        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            items = items
                .Where(x => x.Code.Contains(input.FilterText, StringComparison.OrdinalIgnoreCase) ||
                            x.Name.Contains(input.FilterText, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        return new PagedResultDto<TenantDto>
        {
            TotalCount = await _tenantRepository.GetCountAsync(cancellationToken),
            Items = items.Select(MapToDto).ToArray()
        };
    }

    public async Task<TenantDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return MapToDto(await _tenantRepository.GetAsync(id, cancellationToken));
    }

    public async Task<TenantDto> CreateAsync(CreateTenantDto input, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantManager.CreateAsync(
            input.Code,
            input.Name,
            input.Address,
            input.Phone,
            input.RepresentativeName,
            input.ContactEmail,
            cancellationToken);
        await WriteAuditAsync(tenant, AuditAction.Create, "Tenant created.", cancellationToken);
        await PublishTenantCreatedAsync(tenant, cancellationToken);
        return MapToDto(tenant);
    }

    public async Task<TenantDto> CreateInternalAsync(CreateInternalTenantDto input, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantManager.CreateAsync(
            input.Code,
            input.Name,
            input.Address,
            input.Phone,
            input.RepresentativeName,
            input.ContactEmail,
            cancellationToken);

        foreach (var module in input.DefaultModules)
        {
            tenant.EnableModule(module, null, DateTimeOffset.UtcNow);
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await WriteAuditAsync(tenant, AuditAction.Create, "Tenant created via internal onboarding.", cancellationToken);
        await PublishTenantCreatedAsync(tenant, cancellationToken);
        return MapToDto(tenant);
    }

    public async Task<TenantDto> UpdateProfileAsync(Guid id, UpdateTenantProfileDto input, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(id, cancellationToken);
        tenant.UpdateProfile(
            input.Name,
            input.Address,
            input.Phone,
            input.RepresentativeName,
            input.ContactEmail,
            CurrentUser.Id,
            DateTimeOffset.UtcNow);
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await WriteAuditAsync(tenant, AuditAction.Update, "Tenant profile updated.", cancellationToken);
        return MapToDto(tenant);
    }

    public async Task<bool> IsCodeAvailableAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = TenantAggregate.NormalizeCode(code);
        var existing = await _tenantRepository.FindByCodeAsync(normalized, cancellationToken);
        return existing is null;
    }

    public async Task<TenantDto> UpdateSettingsAsync(Guid id, UpdateTenantSettingsDto input, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(id, cancellationToken);
        foreach (var setting in input.Settings)
        {
            tenant.SetSetting(setting.Key, setting.Value, CurrentUser.Id, DateTimeOffset.UtcNow);
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await WriteAuditAsync(tenant, AuditAction.Update, "Tenant settings updated.", cancellationToken);
        return MapToDto(tenant);
    }

    public async Task<TenantDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(id, cancellationToken);
        tenant.Activate(CurrentUser.Id, DateTimeOffset.UtcNow);
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await WriteAuditAsync(tenant, AuditAction.Update, "Tenant activated.", cancellationToken);
        await _eventBus.PublishAsync(new TenantActivatedIntegrationEvent(Guid.NewGuid(), tenant.Id, DateTimeOffset.UtcNow, ServiceName, CorrelationContext.CorrelationId, tenant.Id, tenant.Code, tenant.Name), cancellationToken);
        return MapToDto(tenant);
    }

    public async Task<TenantDto> SuspendAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(id, cancellationToken);
        tenant.Suspend(CurrentUser.Id, DateTimeOffset.UtcNow);
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await WriteAuditAsync(tenant, AuditAction.Update, "Tenant suspended.", cancellationToken);
        await _eventBus.PublishAsync(new TenantSuspendedIntegrationEvent(Guid.NewGuid(), tenant.Id, DateTimeOffset.UtcNow, ServiceName, CorrelationContext.CorrelationId, tenant.Id, tenant.Code, tenant.Name), cancellationToken);
        return MapToDto(tenant);
    }

    public async Task<TenantDto> EnableModuleAsync(Guid id, ChangeTenantModuleDto input, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(id, cancellationToken);
        tenant.EnableModule(input.ModuleCode, CurrentUser.Id, DateTimeOffset.UtcNow);
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await WriteAuditAsync(tenant, AuditAction.Update, $"Tenant module '{input.ModuleCode}' enabled.", cancellationToken);
        return MapToDto(tenant);
    }

    public async Task<TenantDto> DisableModuleAsync(Guid id, ChangeTenantModuleDto input, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(id, cancellationToken);
        tenant.DisableModule(input.ModuleCode, CurrentUser.Id, DateTimeOffset.UtcNow);
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await WriteAuditAsync(tenant, AuditAction.Update, $"Tenant module '{input.ModuleCode}' disabled.", cancellationToken);
        return MapToDto(tenant);
    }

    private Task WriteAuditAsync(TenantAggregate tenant, AuditAction action, string summary, CancellationToken cancellationToken)
    {
        return _auditWriter.WriteAsync(new AuditLogEntry(Guid.NewGuid(), tenant.Id, CurrentUser.Id, ServiceName, nameof(TenantAggregate), tenant.Id.ToString(), action, summary, CorrelationContext.CorrelationId, DateTimeOffset.UtcNow), cancellationToken);
    }

    private Task PublishTenantCreatedAsync(TenantAggregate tenant, CancellationToken cancellationToken)
    {
        return _eventBus.PublishAsync(new TenantCreatedIntegrationEvent(
            Guid.NewGuid(),
            tenant.Id,
            DateTimeOffset.UtcNow,
            ServiceName,
            CorrelationContext.CorrelationId,
            tenant.Id,
            tenant.Code,
            tenant.Name,
            tenant.ContactEmail,
            tenant.RepresentativeName), cancellationToken);
    }

    private static TenantDto MapToDto(TenantAggregate tenant)
    {
        return new TenantDto
        {
            Id = tenant.Id,
            Code = tenant.Code,
            Name = tenant.Name,
            Address = tenant.Address,
            Phone = tenant.Phone,
            RepresentativeName = tenant.RepresentativeName,
            ContactEmail = tenant.ContactEmail,
            Status = tenant.Status.ToString(),
            ConcurrencyStamp = tenant.ConcurrencyStamp,
            Modules = tenant.Modules.Select(x => new TenantModuleDto { ModuleCode = x.ModuleCode, IsEnabled = x.IsEnabled }).ToArray(),
            Settings = tenant.Settings.ToDictionary(x => x.Key, x => x.Value)
        };
    }
}
