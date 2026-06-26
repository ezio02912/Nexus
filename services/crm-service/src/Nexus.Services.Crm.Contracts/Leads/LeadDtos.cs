using Nexus.ApiContracts.Dtos;
using Nexus.Services.Crm.Domain.Enums;

namespace Nexus.Services.Crm.Contracts.Leads;

public sealed class GetLeadsInput : PagedAndSortedResultRequestDto
{
    public string? Search { get; init; }
    public LeadStatus? Status { get; init; }
    public Guid? OwnerId { get; init; }
}

public sealed class CreateLeadDto
{
    public required string FullName { get; init; }
    public string? CompanyName { get; init; }
    public string? Title { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Source { get; init; }
    public Guid? OwnerId { get; init; }
}

public sealed class UpdateLeadDto
{
    public required string FullName { get; init; }
    public string? CompanyName { get; init; }
    public string? Title { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Source { get; init; }
    public int LeadScore { get; init; }
    public LeadRating? Rating { get; init; }
    public LeadStatus Status { get; init; }
    public Guid? OwnerId { get; init; }
    public string? Description { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public string? LostReason { get; init; }
}

public sealed class LeadDto
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? CompanyName { get; init; }
    public string? Title { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Source { get; init; }
    public LeadStatus Status { get; init; }
    public int LeadScore { get; init; }
    public LeadRating? Rating { get; init; }
    public Guid? OwnerId { get; init; }
    public DateTimeOffset? AssignedAt { get; init; }
    public Guid? ConvertedCustomerId { get; init; }
    public Guid? ConvertedOpportunityId { get; init; }
    public DateTimeOffset? ConvertedAt { get; init; }
    public string? LostReason { get; init; }
    public string? Description { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public DateTimeOffset CreationTime { get; init; }
    public Guid? CreatorId { get; init; }
    public DateTimeOffset? LastModificationTime { get; init; }
    public Guid? LastModifierId { get; init; }
    public string ConcurrencyStamp { get; init; } = string.Empty;
}

public sealed class ConvertLeadDto
{
    public required string CustomerCode { get; init; }
    public CustomerType CustomerType { get; init; }
    public required string OpportunityName { get; init; }
    public decimal OpportunityAmount { get; init; }
    public DateOnly? ExpectedCloseDate { get; init; }
    public Guid? OwnerId { get; init; }
}

public sealed class ConvertLeadResultDto
{
    public required Guid LeadId { get; init; }
    public required Guid CustomerId { get; init; }
    public required Guid OpportunityId { get; init; }
    public required LeadDto Lead { get; init; }
}
