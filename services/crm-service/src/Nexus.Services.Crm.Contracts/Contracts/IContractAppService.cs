using Nexus.ApiContracts.Dtos;

namespace Nexus.Services.Crm.Contracts.Contracts;

public interface IContractAppService
{
    Task<PagedResultDto<ContractDto>> GetListAsync(GetContractsInput input, CancellationToken cancellationToken = default);
    Task<ContractDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContractDto> CreateAsync(CreateContractDto input, CancellationToken cancellationToken = default);
    Task<ContractDto> UpdateAsync(Guid id, UpdateContractDto input, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContractDto> SignAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContractDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContractDto> CompleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContractDto> TerminateAsync(Guid id, TerminateContractDto input, CancellationToken cancellationToken = default);
}
