using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.SharedKernel.Events;

namespace Nexus.BuildingBlocks.EntityFrameworkCore.Configurations;

public sealed class EventOutboxMessageConfiguration : IEntityTypeConfiguration<EventOutboxMessage>
{
    public void Configure(EntityTypeBuilder<EventOutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(x => x.EventId);
        builder.Property(x => x.EventName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.SourceService).HasMaxLength(128).IsRequired();
        builder.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.HasIndex(x => x.OccurredAt)
            .HasFilter("published_at IS NULL")
            .HasDatabaseName("ix_outbox_unpublished");
    }
}
