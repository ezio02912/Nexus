namespace Nexus.Services.Crm.Contracts.Dashboard;

public interface ICrmDashboardAppService
{
    Task<CrmDashboardDto> GetAsync(CancellationToken cancellationToken = default);
}
