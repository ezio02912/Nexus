using Nexus.Services.Crm.Domain.Enums;
using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Repositories;
using Nexus.SharedKernel.Validation;

namespace Nexus.Services.Crm.Domain.Activities;

public static class ActivityConsts
{
    public const int SubjectMaxLength = 256;
}

public sealed class CrmActivity : FullAuditedAggregateRoot<Guid>
{
    private CrmActivity()
    {
        Subject = string.Empty;
    }

    public CrmActivity(
        Guid id,
        Guid tenantId,
        CrmRelatedEntityType relatedEntityType,
        Guid relatedEntityId,
        CrmActivityType activityType,
        string subject,
        DateTimeOffset activityDate,
        Guid? ownerId,
        Guid? assignedToId,
        Guid? creatorId,
        DateTimeOffset now)
    {
        Id = id;
        RelatedEntityType = relatedEntityType;
        RelatedEntityId = relatedEntityId;
        ActivityType = activityType;
        Subject = Check.Length(Check.NotNullOrWhiteSpace(subject, nameof(subject)), nameof(subject), ActivityConsts.SubjectMaxLength);
        ActivityDate = activityDate;
        OwnerId = ownerId;
        AssignedToId = assignedToId;
        Status = CrmActivityStatus.Planned;
        SetCreationAudit(tenantId, creatorId, now);
    }

    public CrmRelatedEntityType RelatedEntityType { get; private set; }
    public Guid RelatedEntityId { get; private set; }
    public CrmActivityType ActivityType { get; private set; }
    public string Subject { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset ActivityDate { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public CrmActivityStatus Status { get; private set; }
    public Guid? OwnerId { get; private set; }
    public Guid? AssignedToId { get; private set; }
    public int? DurationMinutes { get; private set; }

    public void Update(
        CrmActivityType activityType,
        string subject,
        string? description,
        DateTimeOffset activityDate,
        DateTimeOffset? dueDate,
        CrmActivityStatus status,
        Guid? ownerId,
        Guid? assignedToId,
        int? durationMinutes,
        Guid? modifierId,
        DateTimeOffset now)
    {
        ActivityType = activityType;
        Subject = Check.Length(Check.NotNullOrWhiteSpace(subject, nameof(subject)), nameof(subject), ActivityConsts.SubjectMaxLength);
        Description = description?.Trim();
        ActivityDate = activityDate;
        DueDate = dueDate;
        Status = status;
        OwnerId = ownerId;
        AssignedToId = assignedToId;
        DurationMinutes = durationMinutes;
        SetModificationAudit(modifierId, now);
    }

    public void Complete(Guid? modifierId, DateTimeOffset now)
    {
        Status = CrmActivityStatus.Completed;
        CompletedAt = now;
        SetModificationAudit(modifierId, now);
    }
}

public interface IActivityRepository : IRepository<CrmActivity, Guid>
{
    Task<IReadOnlyList<CrmActivity>> GetListByTenantAsync(Guid tenantId, CrmRelatedEntityType? relatedEntityType, Guid? relatedEntityId, string? status, int skipCount, int maxResultCount, CancellationToken cancellationToken = default);
    Task<long> GetCountByTenantAsync(Guid tenantId, CrmRelatedEntityType? relatedEntityType, Guid? relatedEntityId, string? status, CancellationToken cancellationToken = default);
}
