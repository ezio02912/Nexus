using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.SharedKernel.Domain;

namespace Nexus.Services.Numbering.Api.Persistence;

public sealed class NumberSequence : NexusEntity<Guid>
{
    private NumberSequence()
    {
        SequenceKey = string.Empty;
    }

    public NumberSequence(Guid id, string sequenceKey, long currentValue)
    {
        Id = id;
        SequenceKey = sequenceKey;
        CurrentValue = currentValue;
    }

    public string SequenceKey { get; private set; }
    public long CurrentValue { get; private set; }
}

public sealed class NumberingDbContext : NexusDbContext
{
    public NumberingDbContext(DbContextOptions<NumberingDbContext> options) : base(options)
    {
    }

    public DbSet<NumberSequence> Sequences => Set<NumberSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NumberSequence>(builder =>
        {
            builder.ToTable("number_sequences");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.SequenceKey).HasMaxLength(256).IsRequired();
            builder.HasIndex(x => x.SequenceKey).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}
