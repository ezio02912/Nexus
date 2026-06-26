using Nexus.ApiContracts.Dtos;

namespace Nexus.Services.Crm.Contracts.Activities;

public interface IActivityAppService
{
    Task<PagedResultDto<ActivityDto>> GetListAsync(GetActivitiesInput input, CancellationToken cancellationToken = default);
    Task<ActivityDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ActivityDto> CreateAsync(CreateActivityDto input, CancellationToken cancellationToken = default);
    Task<ActivityDto> UpdateAsync(Guid id, UpdateActivityDto input, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ActivityDto> CompleteAsync(Guid id, CancellationToken cancellationToken = default);
}
