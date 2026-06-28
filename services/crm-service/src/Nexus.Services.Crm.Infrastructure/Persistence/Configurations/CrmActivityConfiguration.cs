using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Services.Crm.Domain.Activities;

namespace Nexus.Services.Crm.Infrastructure.Persistence.Configurations;

public sealed class CrmActivityConfiguration : IEntityTypeConfiguration<CrmActivity>
{
    public void Configure(EntityTypeBuilder<CrmActivity> builder)
    {
        builder.ToTable("crm_activities");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RelatedEntityType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ActivityType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(ActivityConsts.SubjectMaxLength).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.AssignedToIds).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(64).IsConcurrencyToken();

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.RelatedEntityType, x.RelatedEntityId });
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.ActivityDate });
    }
}
