using Nexus.ApiContracts.Dtos;

namespace Nexus.Services.Crm.Contracts.Opportunities;

public interface IOpportunityAppService
{
    Task<PagedResultDto<OpportunityDto>> GetListAsync(GetOpportunitiesInput input, CancellationToken cancellationToken = default);
    Task<OpportunityDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OpportunityDto> CreateAsync(CreateOpportunityDto input, CancellationToken cancellationToken = default);
    Task<OpportunityDto> UpdateAsync(Guid id, UpdateOpportunityDto input, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OpportunityDto> ChangeStageAsync(Guid id, ChangeOpportunityStageDto input, CancellationToken cancellationToken = default);
}
