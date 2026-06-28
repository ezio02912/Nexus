using Nexus.ApiContracts.Dtos;
using Nexus.Services.Crm.Contracts.Activities;
using Nexus.Services.Crm.Domain.Activities;
using Nexus.Services.Crm.Domain.Enums;
using Nexus.SharedKernel.Context;

namespace Nexus.Services.Crm.Application.Activities;

public sealed class ActivityAppService : CrmAppServiceBase, IActivityAppService
{
    private readonly IActivityRepository _activityRepository;

    public ActivityAppService(
        IActivityRepository activityRepository,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        ICorrelationContext correlationContext)
        : base(currentTenant, currentUser, correlationContext)
    {
        _activityRepository = activityRepository;
    }

    public async Task<PagedResultDto<ActivityDto>> GetListAsync(GetActivitiesInput input, CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        var status = input.Status?.ToString();

        var items = await _activityRepository.GetListByTenantAsync(
            tenantId,
            input.RelatedEntityType,
            input.RelatedEntityId,
            status,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        return new PagedResultDto<ActivityDto>
        {
            TotalCount = await _activityRepository.GetCountByTenantAsync(tenantId, input.RelatedEntityType, input.RelatedEntityId, status, cancellationToken),
            Items = items.Select(MapToDto).ToArray()
        };
    }

    public async Task<ActivityDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetAsync(id, cancellationToken);
        EnsureTenantAccess(activity);
        return MapToDto(activity);
    }

    public async Task<ActivityDto> CreateAsync(CreateActivityDto input, CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        var now = DateTimeOffset.UtcNow;

        var activity = new CrmActivity(
            Guid.NewGuid(),
            tenantId,
            input.RelatedEntityType,
            input.RelatedEntityId,
            input.ActivityType,
            input.Subject,
            input.ActivityDate,
            input.OwnerId,
            input.AssignedToId,
            input.AssignedToIds,
            CurrentUser.Id,
            now);

        await _activityRepository.InsertAsync(activity, cancellationToken);
        return MapToDto(activity);
    }

    public async Task<ActivityDto> UpdateAsync(Guid id, UpdateActivityDto input, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetAsync(id, cancellationToken);
        EnsureTenantAccess(activity);

        var now = DateTimeOffset.UtcNow;
        activity.Update(
            input.ActivityType,
            input.Subject,
            input.Description,
            input.ActivityDate,
            input.DueDate,
            input.Status,
            input.OwnerId,
            input.AssignedToId,
            input.AssignedToIds,
            input.DurationMinutes,
            CurrentUser.Id,
            now);

        await _activityRepository.UpdateAsync(activity, cancellationToken);
        return MapToDto(activity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.FindAsync(id, cancellationToken);
        if (activity is null)
        {
            return;
        }

        EnsureTenantAccess(activity);
        await _activityRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<ActivityDto> CompleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetAsync(id, cancellationToken);
        EnsureTenantAccess(activity);

        if (activity.Status == CrmActivityStatus.Completed)
        {
            return MapToDto(activity);
        }

        var now = DateTimeOffset.UtcNow;
        activity.Complete(CurrentUser.Id, now);
        await _activityRepository.UpdateAsync(activity, cancellationToken);
        return MapToDto(activity);
    }

    private static ActivityDto MapToDto(CrmActivity activity)
    {
        return new ActivityDto
        {
            Id = activity.Id,
            TenantId = activity.TenantId,
            RelatedEntityType = activity.RelatedEntityType,
            RelatedEntityId = activity.RelatedEntityId,
            ActivityType = activity.ActivityType,
            Subject = activity.Subject,
            Description = activity.Description,
            ActivityDate = activity.ActivityDate,
            DueDate = activity.DueDate,
            CompletedAt = activity.CompletedAt,
            Status = activity.Status,
            OwnerId = activity.OwnerId,
            AssignedToId = activity.AssignedToId,
            AssignedToIds = activity.GetAssignedToIds(),
            DurationMinutes = activity.DurationMinutes,
            CreationTime = activity.CreationTime,
            CreatorId = activity.CreatorId,
            LastModificationTime = activity.LastModificationTime,
            LastModifierId = activity.LastModifierId,
            ConcurrencyStamp = activity.ConcurrencyStamp
        };
    }
}
