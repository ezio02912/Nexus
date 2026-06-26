using Nexus.Services.Crm.Contracts.Dashboard;
using Nexus.Services.Crm.Domain.Enums;
using Nexus.Services.Crm.Domain.Contracts;
using Nexus.Services.Crm.Domain.Leads;
using Nexus.Services.Crm.Domain.Opportunities;
using Nexus.Services.Crm.Domain.Quotations;
using Nexus.SharedKernel.Context;

namespace Nexus.Services.Crm.Application.Dashboard;

public sealed class CrmDashboardAppService : CrmAppServiceBase, ICrmDashboardAppService
{
    private const int ExpiringWithinDays = 30;

    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IQuotationRepository _quotationRepository;
    private readonly IContractRepository _contractRepository;

    public CrmDashboardAppService(
        IOpportunityRepository opportunityRepository,
        ILeadRepository leadRepository,
        IQuotationRepository quotationRepository,
        IContractRepository contractRepository,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        ICorrelationContext correlationContext)
        : base(currentTenant, currentUser, correlationContext)
    {
        _opportunityRepository = opportunityRepository;
        _leadRepository = leadRepository;
        _quotationRepository = quotationRepository;
        _contractRepository = contractRepository;
    }

    public async Task<CrmDashboardDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();

        var pipelineValue = await _opportunityRepository.GetOpenPipelineValueAsync(tenantId, cancellationToken);
        var newLeadsCount = await _leadRepository.GetCountByTenantAsync(
            tenantId,
            null,
            LeadStatus.New.ToString(),
            null,
            cancellationToken);
        var pendingQuotationsCount = await _quotationRepository.GetPendingApprovalCountAsync(tenantId, cancellationToken);
        var expiringContractsCount = await _contractRepository.GetExpiringCountAsync(tenantId, ExpiringWithinDays, cancellationToken);

        var stageFunnelItems = new List<CrmPipelineFunnelItemDto>();
        foreach (OpportunityStage stage in Enum.GetValues<OpportunityStage>())
        {
            var opportunities = await _opportunityRepository.GetListByTenantAsync(
                tenantId,
                null,
                stage.ToString(),
                null,
                null,
                0,
                int.MaxValue,
                cancellationToken);

            stageFunnelItems.Add(new CrmPipelineFunnelItemDto
            {
                Stage = stage,
                Count = opportunities.Count,
                TotalAmount = opportunities.Sum(x => x.Amount)
            });
        }

        return new CrmDashboardDto
        {
            PipelineValue = pipelineValue,
            NewLeadsCount = newLeadsCount,
            PendingQuotationsCount = pendingQuotationsCount,
            ExpiringContractsCount = expiringContractsCount,
            StageFunnelItems = stageFunnelItems
        };
    }
}
