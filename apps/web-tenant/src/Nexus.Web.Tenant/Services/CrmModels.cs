namespace Nexus.Web.Tenant.Services;

public sealed record PagedResultDto<T>(long TotalCount, IReadOnlyList<T> Items);

public enum CustomerType
{
    Company = 0,
    Individual = 1
}

public enum CustomerStatus
{
    Active = 0,
    Inactive = 1,
    Prospect = 2
}

public enum LeadRating
{
    Hot = 0,
    Warm = 1,
    Cold = 2
}

public enum LeadStatus
{
    New = 0,
    Contacted = 1,
    Qualified = 2,
    Unqualified = 3,
    Converted = 4,
    Lost = 5
}

public enum OpportunityStage
{
    Prospecting = 0,
    Qualification = 1,
    Proposal = 2,
    Negotiation = 3,
    ClosedWon = 4,
    ClosedLost = 5
}

public enum QuotationStatus
{
    Draft = 0,
    Sent = 1,
    Approved = 2,
    Rejected = 3,
    Expired = 4,
    Cancelled = 5
}

public enum ContractStatus
{
    Draft = 0,
    PendingSign = 1,
    Signed = 2,
    Active = 3,
    Expired = 4,
    Terminated = 5,
    Cancelled = 6,
    Completed = 7
}

public enum CrmActivityType
{
    Call = 0,
    Email = 1,
    Meeting = 2,
    Task = 3,
    Note = 4
}

public enum CrmActivityStatus
{
    Planned = 0,
    Completed = 1,
    Cancelled = 2
}

public enum CrmRelatedEntityType
{
    Customer = 0,
    Lead = 1,
    Opportunity = 2,
    Quotation = 3,
    Contract = 4
}

// Response DTOs bound to BootstrapBlazor tables use settable properties so Table can bind columns.
public sealed record CustomerDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public CustomerType CustomerType { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public CustomerStatus Status { get; set; }
    public string? TaxCode { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public Guid? OwnerId { get; set; }
    public string? Description { get; set; }
    public LeadRating? Rating { get; set; }
    public string? Source { get; set; }
    public DateTimeOffset CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTimeOffset? LastModificationTime { get; set; }
    public Guid? LastModifierId { get; set; }
    public string ConcurrencyStamp { get; set; } = "";
}

public sealed record CreateCustomerRequest(
    string Code,
    string Name,
    CustomerType CustomerType,
    string? Email,
    string? Phone,
    string? Industry = null,
    string? City = null,
    string? Source = null);

public sealed record UpdateCustomerRequest(
    string Name,
    CustomerType CustomerType,
    string? Email,
    string? Phone,
    string? TaxCode,
    string? Website,
    string? Industry,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    Guid? OwnerId,
    string? Description,
    LeadRating? Rating,
    string? Source,
    CustomerStatus Status);

public sealed record ContactDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Position { get; set; }
    public string? Department { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsDecisionMaker { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Notes { get; set; }
    public Guid? OwnerId { get; set; }
    public DateTimeOffset CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTimeOffset? LastModificationTime { get; set; }
    public Guid? LastModifierId { get; set; }
    public string ConcurrencyStamp { get; set; } = "";
}

public sealed record CreateContactRequest(
    Guid CustomerId,
    string FullName,
    string? Email,
    string? Phone,
    string? Mobile,
    string? Position,
    string? Department,
    bool IsPrimary,
    bool IsDecisionMaker,
    Guid? OwnerId);

public sealed record UpdateContactRequest(
    Guid CustomerId,
    string FullName,
    string? Email,
    string? Phone,
    string? Mobile,
    string? Position,
    string? Department,
    bool IsPrimary,
    bool IsDecisionMaker,
    string? LinkedInUrl,
    string? Notes,
    Guid? OwnerId);

public sealed record LeadDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string FullName { get; set; } = "";
    public string? CompanyName { get; set; }
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Source { get; set; }
    public LeadStatus Status { get; set; }
    public int LeadScore { get; set; }
    public LeadRating? Rating { get; set; }
    public Guid? OwnerId { get; set; }
    public DateTimeOffset? AssignedAt { get; set; }
    public Guid? ConvertedCustomerId { get; set; }
    public Guid? ConvertedOpportunityId { get; set; }
    public DateTimeOffset? ConvertedAt { get; set; }
    public string? LostReason { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public DateTimeOffset CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTimeOffset? LastModificationTime { get; set; }
    public Guid? LastModifierId { get; set; }
    public string ConcurrencyStamp { get; set; } = "";
}

public sealed record CreateLeadRequest(
    string FullName,
    string? CompanyName,
    string? Title,
    string? Email,
    string? Phone,
    string? Source,
    Guid? OwnerId);

public sealed record UpdateLeadRequest(
    string FullName,
    string? CompanyName,
    string? Title,
    string? Email,
    string? Phone,
    string? Source,
    int LeadScore,
    LeadRating? Rating,
    LeadStatus Status,
    Guid? OwnerId,
    string? Description,
    string? Address,
    string? City,
    string? Country,
    string? LostReason);

public sealed record ConvertLeadRequest(
    string CustomerCode,
    CustomerType CustomerType,
    string OpportunityName,
    decimal OpportunityAmount,
    DateOnly? ExpectedCloseDate,
    Guid? OwnerId);

public sealed record ConvertLeadResultDto
{
    public Guid LeadId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid OpportunityId { get; set; }
    public LeadDto Lead { get; set; } = new();
}

public sealed record OpportunityDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? LeadId { get; set; }
    public Guid? ContactId { get; set; }
    public string Name { get; set; } = "";
    public OpportunityStage Stage { get; set; }
    public decimal Amount { get; set; }
    public int Probability { get; set; }
    public string Currency { get; set; } = "";
    public DateOnly? ExpectedCloseDate { get; set; }
    public DateOnly? ActualCloseDate { get; set; }
    public string? CloseReason { get; set; }
    public string? LostReason { get; set; }
    public string? Description { get; set; }
    public string? NextStep { get; set; }
    public DateOnly? NextStepDate { get; set; }
    public string? Source { get; set; }
    public string? Competitor { get; set; }
    public Guid? OwnerId { get; set; }
    public DateTimeOffset CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTimeOffset? LastModificationTime { get; set; }
    public Guid? LastModifierId { get; set; }
    public string ConcurrencyStamp { get; set; } = "";
}

public sealed record CreateOpportunityRequest(
    Guid? CustomerId,
    Guid? LeadId,
    string Name,
    decimal Amount,
    DateOnly? ExpectedCloseDate,
    Guid? OwnerId);

public sealed record UpdateOpportunityRequest(
    Guid? CustomerId,
    Guid? ContactId,
    string Name,
    decimal Amount,
    int Probability,
    string Currency,
    DateOnly? ExpectedCloseDate,
    string? Description,
    string? NextStep,
    DateOnly? NextStepDate,
    string? Source,
    string? Competitor,
    Guid? OwnerId);

public sealed record ChangeOpportunityStageRequest(
    OpportunityStage Stage,
    int? Probability,
    string? CloseReason,
    string? LostReason);

public sealed record QuotationLineDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid QuotationId { get; set; }
    public int LineNo { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal LineTotal { get; set; }
    public int SortOrder { get; set; }
}

public sealed record CreateQuotationLineRequest(
    int LineNo,
    string ProductCode,
    string ProductName,
    string? Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal TaxPercent,
    int SortOrder);

public sealed record QuotationDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? ContactId { get; set; }
    public string QuotationNo { get; set; } = "";
    public string? Subject { get; set; }
    public string? Description { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "";
    public DateOnly? ValidUntil { get; set; }
    public QuotationStatus Status { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public Guid? OwnerId { get; set; }
    public IReadOnlyList<QuotationLineDto> Lines { get; set; } = [];
    public DateTimeOffset CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTimeOffset? LastModificationTime { get; set; }
    public Guid? LastModifierId { get; set; }
    public string ConcurrencyStamp { get; set; } = "";
}

public sealed record CreateQuotationRequest(
    Guid CustomerId,
    string QuotationNo,
    Guid? OpportunityId,
    Guid? ContactId,
    string? Subject,
    Guid? OwnerId,
    IReadOnlyList<CreateQuotationLineRequest> Lines);

public sealed record UpdateQuotationRequest(
    Guid CustomerId,
    Guid? OpportunityId,
    Guid? ContactId,
    string? Subject,
    string? Description,
    decimal DiscountAmount,
    decimal DiscountPercent,
    DateOnly? ValidUntil,
    string? Notes,
    string? Terms,
    Guid? OwnerId,
    IReadOnlyList<CreateQuotationLineRequest> Lines);

public sealed record RejectQuotationRequest(string? Reason);

public sealed record ContractLineDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ContractId { get; set; }
    public int LineNo { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal LineTotal { get; set; }
    public int SortOrder { get; set; }
}

public sealed record CreateContractLineRequest(
    int LineNo,
    string ProductCode,
    string ProductName,
    string? Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal TaxPercent,
    int SortOrder);

public sealed record ContractDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? QuotationId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? ContactId { get; set; }
    public string ContractNo { get; set; } = "";
    public string Title { get; set; } = "";
    public decimal ContractValue { get; set; }
    public string Currency { get; set; } = "";
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly? RenewalDate { get; set; }
    public ContractStatus Status { get; set; }
    public DateTimeOffset? SignedAt { get; set; }
    public Guid? SignedBy { get; set; }
    public string? TerminationReason { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public Guid? FileId { get; set; }
    public Guid? OwnerId { get; set; }
    public IReadOnlyList<ContractLineDto> Lines { get; set; } = [];
    public DateTimeOffset CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTimeOffset? LastModificationTime { get; set; }
    public Guid? LastModifierId { get; set; }
    public string ConcurrencyStamp { get; set; } = "";
}

public sealed record CreateContractRequest(
    Guid CustomerId,
    string ContractNo,
    string Title,
    Guid? QuotationId,
    Guid? OpportunityId,
    Guid? ContactId,
    decimal ContractValue,
    string Currency,
    DateOnly? StartDate,
    DateOnly? EndDate,
    DateOnly? RenewalDate,
    string? PaymentTerms,
    string? Notes,
    string? Terms,
    Guid? FileId,
    Guid? OwnerId,
    IReadOnlyList<CreateContractLineRequest> Lines);

public sealed record UpdateContractRequest(
    Guid CustomerId,
    Guid? QuotationId,
    Guid? OpportunityId,
    Guid? ContactId,
    string Title,
    decimal ContractValue,
    string Currency,
    DateOnly? StartDate,
    DateOnly? EndDate,
    DateOnly? RenewalDate,
    string? PaymentTerms,
    string? Notes,
    string? Terms,
    Guid? FileId,
    Guid? OwnerId,
    IReadOnlyList<CreateContractLineRequest> Lines);

public sealed record TerminateContractRequest(string? Reason);

public sealed record ActivityDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public CrmRelatedEntityType RelatedEntityType { get; set; }
    public Guid RelatedEntityId { get; set; }
    public CrmActivityType ActivityType { get; set; }
    public string Subject { get; set; } = "";
    public string? Description { get; set; }
    public DateTimeOffset ActivityDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public CrmActivityStatus Status { get; set; }
    public Guid? OwnerId { get; set; }
    public Guid? AssignedToId { get; set; }
    public IReadOnlyList<Guid> AssignedToIds { get; set; } = [];
    public int? DurationMinutes { get; set; }
    public DateTimeOffset CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTimeOffset? LastModificationTime { get; set; }
    public Guid? LastModifierId { get; set; }
    public string ConcurrencyStamp { get; set; } = "";
}

public sealed record CreateActivityRequest(
    CrmRelatedEntityType RelatedEntityType,
    Guid RelatedEntityId,
    CrmActivityType ActivityType,
    string Subject,
    DateTimeOffset ActivityDate,
    Guid? OwnerId,
    Guid? AssignedToId,
    IReadOnlyList<Guid>? AssignedToIds);

public sealed record UpdateActivityRequest(
    CrmActivityType ActivityType,
    string Subject,
    string? Description,
    DateTimeOffset ActivityDate,
    DateTimeOffset? DueDate,
    CrmActivityStatus Status,
    Guid? OwnerId,
    Guid? AssignedToId,
    IReadOnlyList<Guid>? AssignedToIds,
    int? DurationMinutes);

public sealed record CrmPipelineFunnelItemDto
{
    public OpportunityStage Stage { get; set; }
    public long Count { get; set; }
    public decimal TotalAmount { get; set; }
}

public sealed record CrmDashboardDto
{
    public decimal PipelineValue { get; set; }
    public long NewLeadsCount { get; set; }
    public long PendingQuotationsCount { get; set; }
    public long ExpiringContractsCount { get; set; }
    public IReadOnlyList<CrmPipelineFunnelItemDto> StageFunnelItems { get; set; } = [];
}

public class CrmListQuery
{
    public string? Search { get; init; }
    public int SkipCount { get; init; }
    public int MaxResultCount { get; init; } = 10;
    public string? Sorting { get; init; }
}

public sealed class CustomerListQuery : CrmListQuery
{
    public CustomerStatus? Status { get; init; }
    public Guid? OwnerId { get; init; }
}

public sealed class ContactListQuery : CrmListQuery
{
    public Guid? CustomerId { get; init; }
}

public sealed class LeadListQuery : CrmListQuery
{
    public LeadStatus? Status { get; init; }
    public Guid? OwnerId { get; init; }
}

public sealed class OpportunityListQuery : CrmListQuery
{
    public OpportunityStage? Stage { get; init; }
    public Guid? CustomerId { get; init; }
    public Guid? OwnerId { get; init; }
}

public sealed class QuotationListQuery : CrmListQuery
{
    public QuotationStatus? Status { get; init; }
    public Guid? CustomerId { get; init; }
}

public sealed class ContractListQuery : CrmListQuery
{
    public ContractStatus? Status { get; init; }
    public Guid? CustomerId { get; init; }
}

public sealed class ActivityListQuery : CrmListQuery
{
    public CrmRelatedEntityType? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public CrmActivityStatus? Status { get; init; }
}
