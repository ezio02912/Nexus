namespace Nexus.Services.Crm.Contracts.Numbering;

public interface INumberingClient
{
    Task<string> GetNextNumberAsync(Guid tenantId, string module, string documentType, string prefix, CancellationToken cancellationToken = default);
}
