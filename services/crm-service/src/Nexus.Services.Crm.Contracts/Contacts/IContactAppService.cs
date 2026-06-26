using Nexus.ApiContracts.Dtos;

namespace Nexus.Services.Crm.Contracts.Contacts;

public interface IContactAppService
{
    Task<PagedResultDto<ContactDto>> GetListAsync(GetContactsInput input, CancellationToken cancellationToken = default);
    Task<ContactDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContactDto> CreateAsync(CreateContactDto input, CancellationToken cancellationToken = default);
    Task<ContactDto> UpdateAsync(Guid id, UpdateContactDto input, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
