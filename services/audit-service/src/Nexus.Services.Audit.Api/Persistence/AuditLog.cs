using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.SharedKernel.Auditing;
using Nexus.SharedKernel.Domain;

namespace Nexus.Services.Audit.Api.Persistence;

public sealed class AuditLog : NexusEntity<Guid>
{
    private AuditLog()
    {
        ServiceName = string.Empty;
        EntityName = string.Empty;
    }

    public AuditLog(Guid id, Guid? tenantId, Guid? userId, string serviceName, string entityName, string? entityId, AuditAction action, string? summary, string? correlationId, DateTimeOffset occurredAt)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        ServiceName = serviceName;
        EntityName = entityName;
        EntityId = entityId;
        Action = action;
        Summary = summary;
        CorrelationId = correlationId;
        OccurredAt = occurredAt;
    }

    public Guid? TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public string ServiceName { get; private set; }
    public string EntityName { get; private set; }
    public string? EntityId { get; private set; }
    public AuditAction Action { get; private set; }
    public string? Summary { get; private set; }
    public string? CorrelationId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
}

public sealed class AuditDbContext : NexusDbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.ToTable("audit_logs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ServiceName).HasMaxLength(128).IsRequired();
            builder.Property(x => x.EntityName).HasMaxLength(256).IsRequired();
            builder.Property(x => x.EntityId).HasMaxLength(128);
            builder.Property(x => x.Action).HasConversion<string>().HasMaxLength(32);
            builder.HasIndex(x => x.TenantId);
            builder.HasIndex(x => x.OccurredAt);
        });

        base.OnModelCreating(modelBuilder);
    }
}
