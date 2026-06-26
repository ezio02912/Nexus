using Nexus.ApiContracts.Dtos;
using Nexus.Services.Crm.Domain.Enums;

namespace Nexus.Services.Crm.Contracts.Activities;

public sealed class GetActivitiesInput : PagedAndSortedResultRequestDto
{
    public CrmRelatedEntityType? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public CrmActivityStatus? Status { get; init; }
}

public sealed class CreateActivityDto
{
    public CrmRelatedEntityType RelatedEntityType { get; init; }
    public required Guid RelatedEntityId { get; init; }
    public CrmActivityType ActivityType { get; init; }
    public required string Subject { get; init; }
    public DateTimeOffset ActivityDate { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? AssignedToId { get; init; }
}

public sealed class UpdateActivityDto
{
    public CrmActivityType ActivityType { get; init; }
    public required string Subject { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset ActivityDate { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public CrmActivityStatus Status { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? AssignedToId { get; init; }
    public int? DurationMinutes { get; init; }
}

public sealed class ActivityDto
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public CrmRelatedEntityType RelatedEntityType { get; init; }
    public Guid RelatedEntityId { get; init; }
    public CrmActivityType ActivityType { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTimeOffset ActivityDate { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public CrmActivityStatus Status { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? AssignedToId { get; init; }
    public int? DurationMinutes { get; init; }
    public DateTimeOffset CreationTime { get; init; }
    public Guid? CreatorId { get; init; }
    public DateTimeOffset? LastModificationTime { get; init; }
    public Guid? LastModifierId { get; init; }
    public string ConcurrencyStamp { get; init; } = string.Empty;
}
