namespace Nexus.SharedKernel.Domain;

/// <summary>
/// Marks an entity that is removed by flagging instead of physical deletion.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; }

    void MarkDeleted(Guid? deleterId, DateTimeOffset now);
}
