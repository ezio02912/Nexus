using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Services.Identity.Domain.Onboarding;
using Nexus.Services.Identity.Domain.Users;

namespace Nexus.Services.Identity.Infrastructure.Persistence;

public sealed class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.ToTable("external_logins");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Provider).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ProviderKey).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => new { x.Provider, x.ProviderKey }).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}

public sealed class OnboardingSessionConfiguration : IEntityTypeConfiguration<OnboardingSession>
{
    public void Configure(EntityTypeBuilder<OnboardingSession> builder)
    {
        builder.ToTable("onboarding_sessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Token).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(UserConsts.EmailMaxLength).IsRequired();
        builder.Property(x => x.GoogleSub).HasMaxLength(256).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.Token).IsUnique();
    }
}

public sealed class TenantRegistrationConfiguration : IEntityTypeConfiguration<TenantRegistration>
{
    public void Configure(EntityTypeBuilder<TenantRegistration> builder)
    {
        builder.ToTable("tenant_registrations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(UserConsts.EmailMaxLength).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.TenantId).IsUnique();
    }
}
