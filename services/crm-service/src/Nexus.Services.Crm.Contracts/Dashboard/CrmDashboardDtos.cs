using Nexus.Services.Crm.Domain.Enums;

namespace Nexus.Services.Crm.Contracts.Dashboard;

public sealed class CrmDashboardDto
{
    public decimal PipelineValue { get; init; }
    public long NewLeadsCount { get; init; }
    public long PendingQuotationsCount { get; init; }
    public long ExpiringContractsCount { get; init; }
    public IReadOnlyList<CrmPipelineFunnelItemDto> StageFunnelItems { get; init; } = [];
}

public sealed class CrmPipelineFunnelItemDto
{
    public OpportunityStage Stage { get; init; }
    public long Count { get; init; }
    public decimal TotalAmount { get; init; }
}
