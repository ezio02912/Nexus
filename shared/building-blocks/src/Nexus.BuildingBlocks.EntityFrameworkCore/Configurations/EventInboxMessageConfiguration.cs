using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.SharedKernel.Events;

namespace Nexus.BuildingBlocks.EntityFrameworkCore.Configurations;

public sealed class EventInboxMessageConfiguration : IEntityTypeConfiguration<EventInboxMessage>
{
    public void Configure(EntityTypeBuilder<EventInboxMessage> builder)
    {
        builder.ToTable("inbox_messages");
        builder.HasKey(x => x.EventId);
        builder.Property(x => x.EventName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.SourceService).HasMaxLength(128).IsRequired();
        builder.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ReceivedAt).IsRequired();
    }
}
