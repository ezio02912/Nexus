using Nexus.Services.Tenant.Domain.Tenants;
using DomainTenant = Nexus.Services.Tenant.Domain.Tenants.Tenant;

namespace Nexus.Services.Tenant.Domain.Tests.Tenants;

public sealed class TenantTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly DateTimeOffset Now = new(2026, 6, 27, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_normalizes_code_contact_email_and_optional_profile_fields()
    {
        var tenant = new DomainTenant(
            TenantId,
            "  acme  ",
            "Acme Co",
            "  123 Main  ",
            "  0909  ",
            "  Jane Doe  ",
            "  Admin@Example.COM  ",
            UserId,
            Now);

        Assert.Equal("ACME", tenant.Code);
        Assert.Equal("Acme Co", tenant.Name);
        Assert.Equal("123 Main", tenant.Address);
        Assert.Equal("0909", tenant.Phone);
        Assert.Equal("Jane Doe", tenant.RepresentativeName);
        Assert.Equal("admin@example.com", tenant.ContactEmail);
        Assert.Equal(TenantStatus.Active, tenant.Status);
        Assert.Equal(TenantId, tenant.TenantId);
        Assert.Equal(UserId, tenant.CreatorId);
        Assert.Equal(Now, tenant.CreationTime);
    }

    [Fact]
    public void UpdateProfile_normalizes_values_and_updates_audit_fields()
    {
        var tenant = CreateTenant();
        var updatedAt = Now.AddHours(1);

        tenant.UpdateProfile(
            "Acme Vietnam",
            "  District 1  ",
            "  0123456789  ",
            "  John Smith  ",
            "  Owner@Example.COM  ",
            UserId,
            updatedAt);

        Assert.Equal("Acme Vietnam", tenant.Name);
        Assert.Equal("District 1", tenant.Address);
        Assert.Equal("0123456789", tenant.Phone);
        Assert.Equal("John Smith", tenant.RepresentativeName);
        Assert.Equal("owner@example.com", tenant.ContactEmail);
        Assert.Equal(UserId, tenant.LastModifierId);
        Assert.Equal(updatedAt, tenant.LastModificationTime);
    }

    [Fact]
    public void EnableModule_adds_once_and_reenables_existing_module()
    {
        var tenant = CreateTenant();

        tenant.EnableModule(" crm ", UserId, Now);
        tenant.DisableModule("CRM", UserId, Now.AddMinutes(1));
        tenant.EnableModule("CRM", UserId, Now.AddMinutes(2));

        var module = Assert.Single(tenant.Modules);
        Assert.Equal("CRM", module.ModuleCode);
        Assert.True(module.IsEnabled);
    }

    [Fact]
    public void SetSetting_adds_once_and_updates_existing_value()
    {
        var tenant = CreateTenant();

        tenant.SetSetting("timezone", "Asia/Bangkok", UserId, Now);
        tenant.SetSetting("timezone", "UTC", UserId, Now.AddMinutes(1));

        var setting = Assert.Single(tenant.Settings);
        Assert.Equal("timezone", setting.Key);
        Assert.Equal("UTC", setting.Value);
    }

    [Fact]
    public void SetSubscription_creates_then_updates_existing_subscription()
    {
        var tenant = CreateTenant();
        var firstExpiry = Now.AddDays(30);
        var secondExpiry = Now.AddDays(60);

        tenant.SetSubscription(" basic ", firstExpiry, UserId, Now);
        var originalSubscription = tenant.Subscription;
        tenant.SetSubscription(" pro ", secondExpiry, UserId, Now.AddMinutes(1));

        Assert.Same(originalSubscription, tenant.Subscription);
        Assert.Equal("PRO", tenant.Subscription!.PlanCode);
        Assert.Equal(secondExpiry, tenant.Subscription.ExpiresAt);
    }

    private static DomainTenant CreateTenant()
    {
        return new DomainTenant(TenantId, "ACME", "Acme Co", UserId, Now);
    }
}
