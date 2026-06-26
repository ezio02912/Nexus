using Nexus.SharedKernel.MultiTenancy;

namespace Nexus.SharedKernel.Domain;

public abstract class FullAuditedAggregateRoot<TKey> : NexusAggregateRoot<TKey>, IFullAuditedObject, IMultiTenant, IHasConcurrencyStamp, ISoftDelete
{
    public Guid? TenantId { get; protected set; }
    public DateTimeOffset CreationTime { get; protected set; }
    public Guid? CreatorId { get; protected set; }
    public DateTimeOffset? LastModificationTime { get; protected set; }
    public Guid? LastModifierId { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public DateTimeOffset? DeletionTime { get; protected set; }
    public Guid? DeleterId { get; protected set; }
    public string ConcurrencyStamp { get; protected set; } = Guid.NewGuid().ToString("N");

    protected void SetCreationAudit(Guid? tenantId, Guid? creatorId, DateTimeOffset now)
    {
        TenantId = tenantId;
        CreatorId = creatorId;
        CreationTime = now;
    }

    public void SetModificationAudit(Guid? modifierId, DateTimeOffset now)
    {
        LastModifierId = modifierId;
        LastModificationTime = now;
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }

    public void MarkDeleted(Guid? deleterId, DateTimeOffset now)
    {
        IsDeleted = true;
        DeleterId = deleterId;
        DeletionTime = now;
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }
}
