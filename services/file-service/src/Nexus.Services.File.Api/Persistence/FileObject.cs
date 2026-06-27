using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.SharedKernel.Domain;

namespace Nexus.Services.File.Api.Persistence;

public sealed class FileObject : NexusEntity<Guid>
{
    private readonly List<FileLink> _links = [];

    private FileObject()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        StoragePath = string.Empty;
    }

    public FileObject(Guid id, Guid? tenantId, string fileName, string contentType, long size, string storagePath, DateTimeOffset createdAt)
    {
        Id = id;
        TenantId = tenantId;
        FileName = fileName;
        ContentType = contentType;
        Size = size;
        StoragePath = storagePath;
        CreatedAt = createdAt;
    }

    public Guid? TenantId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public long Size { get; private set; }
    public string StoragePath { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<FileLink> Links => _links.AsReadOnly();

    public FileLink AddLink(string module, string entityType, string entityId, string? category, DateTimeOffset now)
    {
        var link = new FileLink(Guid.NewGuid(), Id, module, entityType, entityId, category, now);
        _links.Add(link);
        return link;
    }
}

public sealed class FileLink : NexusEntity<Guid>
{
    private FileLink()
    {
        Module = string.Empty;
        EntityType = string.Empty;
        EntityId = string.Empty;
    }

    public FileLink(Guid id, Guid fileId, string module, string entityType, string entityId, string? category, DateTimeOffset createdAt)
    {
        Id = id;
        FileId = fileId;
        Module = module;
        EntityType = entityType;
        EntityId = entityId;
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        CreatedAt = createdAt;
    }

    public Guid FileId { get; private set; }
    public string Module { get; private set; }
    public string EntityType { get; private set; }
    public string EntityId { get; private set; }

    // Business document type of the attachment (e.g. SALES_INVOICE, PURCHASE_RECEIPT).
    // Nullable so legacy links remain valid.
    public string? Category { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}

public sealed class FileDbContext : NexusDbContext
{
    public FileDbContext(DbContextOptions<FileDbContext> options) : base(options)
    {
    }

    public DbSet<FileObject> Files => Set<FileObject>();

    public DbSet<FileLink> FileLinks => Set<FileLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileObject>(builder =>
        {
            builder.ToTable("files");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FileName).HasMaxLength(512).IsRequired();
            builder.Property(x => x.ContentType).HasMaxLength(256).IsRequired();
            builder.Property(x => x.StoragePath).HasMaxLength(1024).IsRequired();
            var nav = builder.Metadata.FindNavigation(nameof(FileObject.Links))!;
            nav.SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(x => x.Links).WithOne().HasForeignKey(x => x.FileId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FileLink>(builder =>
        {
            builder.ToTable("file_links");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Module).HasMaxLength(128).IsRequired();
            builder.Property(x => x.EntityType).HasMaxLength(128).IsRequired();
            builder.Property(x => x.EntityId).HasMaxLength(128).IsRequired();
            builder.Property(x => x.Category).HasMaxLength(64);
            builder.HasIndex(x => new { x.Module, x.EntityType, x.EntityId, x.Category });
        });

        base.OnModelCreating(modelBuilder);
    }
}
