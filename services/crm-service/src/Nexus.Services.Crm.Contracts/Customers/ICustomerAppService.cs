using Nexus.ApiContracts.Dtos;

namespace Nexus.Services.Crm.Contracts.Customers;

public interface ICustomerAppService
{
    Task<PagedResultDto<CustomerDto>> GetListAsync(GetCustomersInput input, CancellationToken cancellationToken = default);
    Task<CustomerDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerDto> CreateAsync(CreateCustomerDto input, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerDto input, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
