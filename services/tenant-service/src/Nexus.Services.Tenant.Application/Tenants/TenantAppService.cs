using Nexus.ApiContracts.Dtos;
using Nexus.BuildingBlocks.Application;
using Nexus.EventContracts.Tenants;
using Nexus.Services.Tenant.Application.Subscriptions;
using Nexus.Services.Tenant.Contracts.Tenants;
using Nexus.Services.Tenant.Domain.Tenants;
using Nexus.SharedKernel.Auditing;
using Nexus.SharedKernel.Context;
using Nexus.SharedKernel.Events;
using Nexus.SharedKernel.Exceptions;
using TenantAggregate = Nexus.Services.Tenant.Domain.Tenants.Tenant;

namespace Nexus.Services.Tenant.Application.Tenants;

public sealed class TenantAppService : NexusAppServiceBase, ITenantAppService
{
    private const string ServiceName = "tenant-service";
    private readonly ITenantRepository _tenantRepository;
    private readonly TenantManager _tenantManager;
    private readonly IEventBus _eventBus;
    private readonly IAuditWriter _auditWriter;
    private readonly ISubscriptionPlanCatalog _planCatalog;
    private readonly SubscriptionAppService _subscriptionAppService;

    public TenantAppService(
        ITenantRepository tenantRepository,
        TenantManager tenantManager,
        IEventBus eventBus,
        IAuditWriter auditWriter,
        ISubscriptionPlanCatalog planCatalog,
        SubscriptionAppService subscriptionAppService,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        ICorrelationContext correlationContext) : base(currentTenant, currentUser, correlationContext)
    {
        _tenantRepository = tenantRepository;
        _tenantManager = tenantManager;
        _eventBus = eventBus;
        _auditWriter = auditWriter;
        _planCatalog = planCatalog;
        _subscriptionAppService = subscriptionAppService;
    }

    public async Task<PagedResultDto<TenantDto>> GetListAsync(GetTenantsInput input, CancellationToken cancellationToken = default)
    {
        var items = await _tenantRepository.GetListAsync(input.SkipCount, input.MaxResultCount, input.Sorting, cancellationToken);
        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            var term = input.FilterText.Trim().ToLowerInvariant();
            items = items
                .Where(x => x.Code.ToLowerInvariant().Contains(term) ||
                            x.Name.ToLowerInvariant().Contains(term))
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

        await ApplyInitialSubscriptionAsync(tenant, input.PlanCode, null, cancellationToken);
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

        var planCode = string.IsNullOrWhiteSpace(input.PlanCode) ? "FREE" : input.PlanCode;
        await ApplyInitialSubscriptionAsync(tenant, planCode, input.DefaultModules, cancellationToken);
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

    public async Task<TenantDto> ChangeSubscriptionAsync(Guid id, ChangeTenantSubscriptionDto input, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(id, cancellationToken);
        var oldPlanCode = tenant.Subscription?.PlanCode ?? "FREE";
        var plan = _planCatalog.GetRequired(input.PlanCode);
        var now = DateTimeOffset.UtcNow;

        tenant.SetSubscription(plan.PlanCode, plan.MonthlyPrice > 0 ? now.AddDays(30) : null, CurrentUser.Id, now);
        SubscriptionPlanApplier.ApplyModules(tenant, plan, CurrentUser.Id, now);

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await WriteAuditAsync(tenant, AuditAction.Update, $"Tenant subscription changed to '{plan.PlanCode}'.", cancellationToken);

        if (!string.Equals(oldPlanCode, plan.PlanCode, StringComparison.OrdinalIgnoreCase))
        {
            await _eventBus.PublishAsync(new SubscriptionChangedIntegrationEvent(
                Guid.NewGuid(),
                tenant.Id,
                now,
                ServiceName,
                CorrelationContext.CorrelationId,
                tenant.Id,
                oldPlanCode,
                plan.PlanCode,
                plan.MonthlyPrice), cancellationToken);
        }

        return MapToDto(tenant);
    }

    private async Task ApplyInitialSubscriptionAsync(
        TenantAggregate tenant,
        string planCode,
        IReadOnlyList<string>? overrideModules,
        CancellationToken cancellationToken)
    {
        var plan = _planCatalog.GetRequired(planCode);
        var now = DateTimeOffset.UtcNow;
        tenant.SetSubscription(plan.PlanCode, plan.MonthlyPrice > 0 ? now.AddDays(30) : null, null, now);

        if (overrideModules is { Count: > 0 })
        {
            foreach (var module in overrideModules)
            {
                tenant.EnableModule(module, null, now);
            }
        }
        else
        {
            SubscriptionPlanApplier.ApplyModules(tenant, plan, null, now);
        }

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
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

    private TenantDto MapToDto(TenantAggregate tenant)
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
            Subscription = tenant.Subscription is null ? null : _subscriptionAppService.MapSubscription(tenant.Subscription),
            Modules = tenant.Modules.Select(x => new TenantModuleDto { ModuleCode = x.ModuleCode, IsEnabled = x.IsEnabled }).ToArray(),
            Settings = tenant.Settings.ToDictionary(x => x.Key, x => x.Value)
        };
    }
}
