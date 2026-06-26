using Nexus.ApiContracts.Dtos;

namespace Nexus.Services.Crm.Contracts.Quotations;

public interface IQuotationAppService
{
    Task<PagedResultDto<QuotationDto>> GetListAsync(GetQuotationsInput input, CancellationToken cancellationToken = default);
    Task<QuotationDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<QuotationDto> CreateAsync(CreateQuotationDto input, CancellationToken cancellationToken = default);
    Task<QuotationDto> UpdateAsync(Guid id, UpdateQuotationDto input, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<QuotationDto> ApproveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<QuotationDto> RejectAsync(Guid id, RejectQuotationDto input, CancellationToken cancellationToken = default);
    Task<QuotationDto> SendAsync(Guid id, CancellationToken cancellationToken = default);
}
