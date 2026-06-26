using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.SharedKernel.Domain;

namespace Nexus.Services.MasterData.Api.Persistence;

public sealed class LookupItem : NexusEntity<Guid>
{
    private LookupItem()
    {
        Category = string.Empty;
        Code = string.Empty;
        Name = string.Empty;
    }

    public LookupItem(Guid id, string category, string code, string name, int sortOrder, bool isActive)
    {
        Id = id;
        Category = category;
        Code = code;
        Name = name;
        SortOrder = sortOrder;
        IsActive = isActive;
    }

    public string Category { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    public void Update(string code, string name, int sortOrder, bool isActive)
    {
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        SortOrder = sortOrder;
        IsActive = isActive;
    }
}

public sealed class MasterDataDbContext : NexusDbContext
{
    public MasterDataDbContext(DbContextOptions<MasterDataDbContext> options) : base(options)
    {
    }

    public DbSet<LookupItem> LookupItems => Set<LookupItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LookupItem>(builder =>
        {
            builder.ToTable("lookup_items");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Category).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
            builder.HasIndex(x => new { x.Category, x.Code }).IsUnique();
            builder.HasIndex(x => new { x.Category, x.IsActive, x.SortOrder });
        });

        base.OnModelCreating(modelBuilder);
    }
}
