using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.SharedKernel.Domain;

namespace Nexus.Services.Workflow.Api.Persistence;

public sealed class WorkflowDefinition : NexusEntity<Guid>
{
    private WorkflowDefinition()
    {
        Code = string.Empty;
        Name = string.Empty;
        Steps = [];
    }

    public WorkflowDefinition(Guid id, Guid? tenantId, string code, string name, List<string> steps, DateTimeOffset createdAt)
    {
        Id = id;
        TenantId = tenantId;
        Code = code.Trim().ToUpperInvariant();
        Name = name;
        Steps = steps;
        IsActive = true;
        CreatedAt = createdAt;
    }

    public Guid? TenantId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public List<string> Steps { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}

public sealed class WorkflowInstance : NexusEntity<Guid>
{
    private readonly List<WorkflowAction> _actions = [];

    private WorkflowInstance()
    {
        SourceModule = string.Empty;
        SourceType = string.Empty;
        SourceId = string.Empty;
        Status = "Pending";
    }

    public WorkflowInstance(Guid id, Guid? tenantId, Guid workflowDefinitionId, string sourceModule, string sourceType, string sourceId, DateTimeOffset createdAt)
    {
        Id = id;
        TenantId = tenantId;
        WorkflowDefinitionId = workflowDefinitionId;
        SourceModule = sourceModule;
        SourceType = sourceType;
        SourceId = sourceId;
        Status = "Pending";
        CreatedAt = createdAt;
    }

    public Guid? TenantId { get; private set; }
    public Guid WorkflowDefinitionId { get; private set; }
    public string SourceModule { get; private set; }
    public string SourceType { get; private set; }
    public string SourceId { get; private set; }
    public string Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<WorkflowAction> Actions => _actions.AsReadOnly();

    public void Approve(Guid userId, string? comment, DateTimeOffset now)
    {
        _actions.Add(new WorkflowAction(Guid.NewGuid(), Id, userId, "Approve", comment, now));
        Status = "Approved";
    }

    public void Reject(Guid userId, string? comment, DateTimeOffset now)
    {
        _actions.Add(new WorkflowAction(Guid.NewGuid(), Id, userId, "Reject", comment, now));
        Status = "Rejected";
    }
}

public sealed class WorkflowAction : NexusEntity<Guid>
{
    private WorkflowAction()
    {
        Action = string.Empty;
    }

    public WorkflowAction(Guid id, Guid workflowInstanceId, Guid userId, string action, string? comment, DateTimeOffset createdAt)
    {
        Id = id;
        WorkflowInstanceId = workflowInstanceId;
        UserId = userId;
        Action = action;
        Comment = comment;
        CreatedAt = createdAt;
    }

    public Guid WorkflowInstanceId { get; private set; }
    public Guid UserId { get; private set; }
    public string Action { get; private set; }
    public string? Comment { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}

public sealed class WorkflowDbContext : NexusDbContext
{
    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options)
    {
    }

    public DbSet<WorkflowDefinition> Definitions => Set<WorkflowDefinition>();

    public DbSet<WorkflowInstance> Instances => Set<WorkflowInstance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var stepsComparer = new ValueComparer<List<string>>(
            (a, b) => a!.SequenceEqual(b!),
            v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            v => v.ToList());

        modelBuilder.Entity<WorkflowDefinition>(builder =>
        {
            builder.ToTable("workflow_definitions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Steps)
                .HasConversion(
                    v => string.Join('\n', v),
                    v => v.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList())
                .Metadata.SetValueComparer(stepsComparer);
            builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        });

        modelBuilder.Entity<WorkflowInstance>(builder =>
        {
            builder.ToTable("workflow_instances");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.SourceModule).HasMaxLength(128).IsRequired();
            builder.Property(x => x.SourceType).HasMaxLength(128).IsRequired();
            builder.Property(x => x.SourceId).HasMaxLength(128).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
            var nav = builder.Metadata.FindNavigation(nameof(WorkflowInstance.Actions))!;
            nav.SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(x => x.Actions).WithOne().HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkflowAction>(builder =>
        {
            builder.ToTable("workflow_actions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Action).HasMaxLength(32).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
