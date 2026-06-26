using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Validation;

namespace Nexus.Services.Tenant.Domain.Tenants;

public sealed class TenantSetting : NexusEntity<Guid>
{
    private TenantSetting()
    {
        Key = string.Empty;
        Value = string.Empty;
    }

    public TenantSetting(Guid id, Guid tenantId, string key, string value)
    {
        Id = id;
        TenantId = tenantId;
        Key = Check.Length(Check.NotNullOrWhiteSpace(key, nameof(key)), nameof(key), TenantConsts.SettingKeyMaxLength);
        Value = value;
    }

    public Guid TenantId { get; private set; }
    public string Key { get; private set; }
    public string Value { get; private set; }

    public void SetValue(string value)
    {
        Value = value;
    }
}
