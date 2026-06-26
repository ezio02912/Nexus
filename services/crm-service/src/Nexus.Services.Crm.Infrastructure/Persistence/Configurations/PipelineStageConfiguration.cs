using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Services.Crm.Domain.PipelineStages;

namespace Nexus.Services.Crm.Infrastructure.Persistence.Configurations;

public sealed class PipelineStageConfiguration : IEntityTypeConfiguration<PipelineStage>
{
    private const int EntityTypeMaxLength = 64;
    private const int CodeMaxLength = 64;
    private const int NameMaxLength = 256;

    public void Configure(EntityTypeBuilder<PipelineStage> builder)
    {
        builder.ToTable("pipeline_stages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType).HasMaxLength(EntityTypeMaxLength).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(CodeMaxLength).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(NameMaxLength).IsRequired();
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(64).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.SortOrder });
    }
}
