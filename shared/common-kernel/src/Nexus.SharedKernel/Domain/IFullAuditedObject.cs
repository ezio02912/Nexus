namespace Nexus.SharedKernel.Domain;

public interface IFullAuditedObject
{
    DateTimeOffset CreationTime { get; }
    Guid? CreatorId { get; }
    DateTimeOffset? LastModificationTime { get; }
    Guid? LastModifierId { get; }
    bool IsDeleted { get; }
    DateTimeOffset? DeletionTime { get; }
    Guid? DeleterId { get; }
}
