using Nexus.ApiContracts.Dtos;
using Nexus.Services.Crm.Domain.Enums;

namespace Nexus.Services.Crm.Contracts.Opportunities;

public sealed class GetOpportunitiesInput : PagedAndSortedResultRequestDto
{
    public string? Search { get; init; }
    public OpportunityStage? Stage { get; init; }
    public Guid? CustomerId { get; init; }
    public Guid? OwnerId { get; init; }
}

public sealed class CreateOpportunityDto
{
    public Guid? CustomerId { get; init; }
    public Guid? LeadId { get; init; }
    public required string Name { get; init; }
    public decimal Amount { get; init; }
    public DateOnly? ExpectedCloseDate { get; init; }
    public Guid? OwnerId { get; init; }
}

public sealed class UpdateOpportunityDto
{
    public Guid? CustomerId { get; init; }
    public Guid? ContactId { get; init; }
    public required string Name { get; init; }
    public decimal Amount { get; init; }
    public int Probability { get; init; }
    public required string Currency { get; init; }
    public DateOnly? ExpectedCloseDate { get; init; }
    public string? Description { get; init; }
    public string? NextStep { get; init; }
    public DateOnly? NextStepDate { get; init; }
    public string? Source { get; init; }
    public string? Competitor { get; init; }
    public Guid? OwnerId { get; init; }
}

public sealed class OpportunityDto
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? CustomerId { get; init; }
    public Guid? LeadId { get; init; }
    public Guid? ContactId { get; init; }
    public string Name { get; init; } = string.Empty;
    public OpportunityStage Stage { get; init; }
    public decimal Amount { get; init; }
    public int Probability { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateOnly? ExpectedCloseDate { get; init; }
    public DateOnly? ActualCloseDate { get; init; }
    public string? CloseReason { get; init; }
    public string? LostReason { get; init; }
    public string? Description { get; init; }
    public string? NextStep { get; init; }
    public DateOnly? NextStepDate { get; init; }
    public string? Source { get; init; }
    public string? Competitor { get; init; }
    public Guid? OwnerId { get; init; }
    public DateTimeOffset CreationTime { get; init; }
    public Guid? CreatorId { get; init; }
    public DateTimeOffset? LastModificationTime { get; init; }
    public Guid? LastModifierId { get; init; }
    public string ConcurrencyStamp { get; init; } = string.Empty;
}

public sealed class ChangeOpportunityStageDto
{
    public OpportunityStage Stage { get; init; }
    public int? Probability { get; init; }
    public string? CloseReason { get; init; }
    public string? LostReason { get; init; }
}
