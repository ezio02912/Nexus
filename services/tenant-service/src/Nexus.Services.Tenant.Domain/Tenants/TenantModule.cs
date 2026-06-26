using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Validation;

namespace Nexus.Services.Tenant.Domain.Tenants;

public sealed class TenantModule : NexusEntity<Guid>
{
    private TenantModule()
    {
        ModuleCode = string.Empty;
    }

    public TenantModule(Guid id, Guid tenantId, string moduleCode, bool isEnabled)
    {
        Id = id;
        TenantId = tenantId;
        ModuleCode = Check.Length(Check.NotNullOrWhiteSpace(moduleCode, nameof(moduleCode)), nameof(moduleCode), TenantConsts.ModuleCodeMaxLength);
        IsEnabled = isEnabled;
    }

    public Guid TenantId { get; private set; }
    public string ModuleCode { get; private set; }
    public bool IsEnabled { get; private set; }

    public void SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}
