using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Validation;

namespace Nexus.Services.Tenant.Domain.Tenants;

public sealed class Tenant : FullAuditedAggregateRoot<Guid>
{
    private readonly List<TenantModule> _modules = [];
    private readonly List<TenantSetting> _settings = [];

    private Tenant()
    {
        Code = string.Empty;
        Name = string.Empty;
    }

    public Tenant(Guid id, string code, string name, Guid? creatorId, DateTimeOffset now)
    {
        Id = id;
        Code = NormalizeCode(code);
        Name = Check.Length(Check.NotNullOrWhiteSpace(name, nameof(name)), nameof(name), TenantConsts.NameMaxLength);
        Status = TenantStatus.Active;
        SetCreationAudit(id, creatorId, now);
    }

    public string Code { get; private set; }
    public string Name { get; private set; }
    public TenantStatus Status { get; private set; }
    public TenantSubscription? Subscription { get; private set; }
    public IReadOnlyCollection<TenantModule> Modules => _modules.AsReadOnly();
    public IReadOnlyCollection<TenantSetting> Settings => _settings.AsReadOnly();

    public void Rename(string name, Guid? modifierId, DateTimeOffset now)
    {
        Name = Check.Length(Check.NotNullOrWhiteSpace(name, nameof(name)), nameof(name), TenantConsts.NameMaxLength);
        SetModificationAudit(modifierId, now);
    }

    public void Activate(Guid? modifierId, DateTimeOffset now)
    {
        Status = TenantStatus.Active;
        SetModificationAudit(modifierId, now);
    }

    public void Suspend(Guid? modifierId, DateTimeOffset now)
    {
        Status = TenantStatus.Suspended;
        SetModificationAudit(modifierId, now);
    }

    public void SetSubscription(string planCode, DateTimeOffset? expiresAt, Guid? modifierId, DateTimeOffset now)
    {
        Subscription = new TenantSubscription(Guid.NewGuid(), Id, NormalizeCode(planCode), expiresAt);
        SetModificationAudit(modifierId, now);
    }

    public void EnableModule(string moduleCode, Guid? modifierId, DateTimeOffset now)
    {
        var normalized = NormalizeCode(moduleCode);
        var module = _modules.SingleOrDefault(x => x.ModuleCode == normalized);
        if (module is null)
        {
            _modules.Add(new TenantModule(Guid.NewGuid(), Id, normalized, true));
        }
        else
        {
            module.SetEnabled(true);
        }

        SetModificationAudit(modifierId, now);
    }

    public void DisableModule(string moduleCode, Guid? modifierId, DateTimeOffset now)
    {
        var normalized = NormalizeCode(moduleCode);
        var module = _modules.SingleOrDefault(x => x.ModuleCode == normalized);
        if (module is not null)
        {
            module.SetEnabled(false);
            SetModificationAudit(modifierId, now);
        }
    }

    public void SetSetting(string key, string value, Guid? modifierId, DateTimeOffset now)
    {
        var settingKey = Check.Length(Check.NotNullOrWhiteSpace(key, nameof(key)), nameof(key), TenantConsts.SettingKeyMaxLength);
        var setting = _settings.SingleOrDefault(x => x.Key == settingKey);
        if (setting is null)
        {
            _settings.Add(new TenantSetting(Guid.NewGuid(), Id, settingKey, value));
        }
        else
        {
            setting.SetValue(value);
        }

        SetModificationAudit(modifierId, now);
    }

    public static string NormalizeCode(string code)
    {
        return Check.Length(Check.NotNullOrWhiteSpace(code, nameof(code)).Trim().ToUpperInvariant(), nameof(code), TenantConsts.CodeMaxLength, TenantConsts.CodeMinLength);
    }
}
