using Nexus.ApiContracts.Dtos;

namespace Nexus.Services.Crm.Contracts.Leads;

public interface ILeadAppService
{
    Task<PagedResultDto<LeadDto>> GetListAsync(GetLeadsInput input, CancellationToken cancellationToken = default);
    Task<LeadDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LeadDto> CreateAsync(CreateLeadDto input, CancellationToken cancellationToken = default);
    Task<LeadDto> UpdateAsync(Guid id, UpdateLeadDto input, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConvertLeadResultDto> ConvertAsync(Guid id, ConvertLeadDto input, CancellationToken cancellationToken = default);
}
