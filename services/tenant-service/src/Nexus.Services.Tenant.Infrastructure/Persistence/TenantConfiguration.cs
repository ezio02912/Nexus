using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Services.Tenant.Domain.Tenants;
using TenantAggregate = Nexus.Services.Tenant.Domain.Tenants.Tenant;

namespace Nexus.Services.Tenant.Infrastructure.Persistence;

public sealed class TenantConfiguration : IEntityTypeConfiguration<TenantAggregate>
{
    public void Configure(EntityTypeBuilder<TenantAggregate> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(TenantConsts.CodeMaxLength).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(TenantConsts.NameMaxLength).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(64).IsConcurrencyToken();

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasOne(x => x.Subscription)
            .WithOne()
            .HasForeignKey<TenantSubscription>(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        var modulesNavigation = builder.Metadata.FindNavigation(nameof(TenantAggregate.Modules))!;
        modulesNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(x => x.Modules)
            .WithOne()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        var settingsNavigation = builder.Metadata.FindNavigation(nameof(TenantAggregate.Settings))!;
        settingsNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(x => x.Settings)
            .WithOne()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class TenantModuleConfiguration : IEntityTypeConfiguration<TenantModule>
{
    public void Configure(EntityTypeBuilder<TenantModule> builder)
    {
        builder.ToTable("tenant_modules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ModuleCode).HasMaxLength(TenantConsts.ModuleCodeMaxLength).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ModuleCode }).IsUnique();
    }
}

public sealed class TenantSettingConfiguration : IEntityTypeConfiguration<TenantSetting>
{
    public void Configure(EntityTypeBuilder<TenantSetting> builder)
    {
        builder.ToTable("tenant_settings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Key).HasMaxLength(TenantConsts.SettingKeyMaxLength).IsRequired();
        builder.Property(x => x.Value).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
    }
}

public sealed class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("tenant_subscriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PlanCode).HasMaxLength(TenantConsts.PlanCodeMaxLength).IsRequired();
        builder.HasIndex(x => x.TenantId).IsUnique();
    }
}
