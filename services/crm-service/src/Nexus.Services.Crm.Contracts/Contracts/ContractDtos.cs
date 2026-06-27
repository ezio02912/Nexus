using Nexus.ApiContracts.Dtos;
using Nexus.Services.Crm.Domain.Enums;

namespace Nexus.Services.Crm.Contracts.Contracts;

public sealed class GetContractsInput : PagedAndSortedResultRequestDto
{
    public string? Search { get; init; }
    public ContractStatus? Status { get; init; }
    public Guid? CustomerId { get; init; }
}

public sealed class CreateContractDto
{
    public required Guid CustomerId { get; init; }
    public required string ContractNo { get; init; }
    public required string Title { get; init; }
    public Guid? QuotationId { get; init; }
    public Guid? OpportunityId { get; init; }
    public Guid? ContactId { get; init; }
    public decimal ContractValue { get; init; }
    public string Currency { get; init; } = "VND";
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public DateOnly? RenewalDate { get; init; }
    public string? PaymentTerms { get; init; }
    public string? Notes { get; init; }
    public string? Terms { get; init; }
    public Guid? FileId { get; init; }
    public Guid? OwnerId { get; init; }
    public IReadOnlyList<CreateContractLineDto> Lines { get; init; } = [];
}

public sealed class UpdateContractDto
{
    public required Guid CustomerId { get; init; }
    public Guid? QuotationId { get; init; }
    public Guid? OpportunityId { get; init; }
    public Guid? ContactId { get; init; }
    public required string Title { get; init; }
    public decimal ContractValue { get; init; }
    public required string Currency { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public DateOnly? RenewalDate { get; init; }
    public string? PaymentTerms { get; init; }
    public string? Notes { get; init; }
    public string? Terms { get; init; }
    public Guid? FileId { get; init; }
    public Guid? OwnerId { get; init; }
    public IReadOnlyList<CreateContractLineDto> Lines { get; init; } = [];
}

public sealed class ContractDto
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public Guid CustomerId { get; init; }
    public Guid? QuotationId { get; init; }
    public Guid? OpportunityId { get; init; }
    public Guid? ContactId { get; init; }
    public string ContractNo { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public decimal ContractValue { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public DateOnly? RenewalDate { get; init; }
    public ContractStatus Status { get; init; }
    public DateTimeOffset? SignedAt { get; init; }
    public Guid? SignedBy { get; init; }
    public string? TerminationReason { get; init; }
    public string? PaymentTerms { get; init; }
    public string? Notes { get; init; }
    public string? Terms { get; init; }
    public Guid? FileId { get; init; }
    public Guid? OwnerId { get; init; }
    public IReadOnlyList<ContractLineDto> Lines { get; init; } = [];
    public DateTimeOffset CreationTime { get; init; }
    public Guid? CreatorId { get; init; }
    public DateTimeOffset? LastModificationTime { get; init; }
    public Guid? LastModifierId { get; init; }
    public string ConcurrencyStamp { get; init; } = string.Empty;
}

public sealed class ContractLineDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid ContractId { get; init; }
    public int LineNo { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Quantity { get; init; }
    public string Unit { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public decimal DiscountPercent { get; init; }
    public decimal TaxPercent { get; init; }
    public decimal LineTotal { get; init; }
    public int SortOrder { get; init; }
}

public sealed class CreateContractLineDto
{
    public int LineNo { get; init; }
    public required string ProductCode { get; init; }
    public required string ProductName { get; init; }
    public string? Description { get; init; }
    public decimal Quantity { get; init; }
    public string Unit { get; init; } = "EA";
    public decimal UnitPrice { get; init; }
    public decimal DiscountPercent { get; init; }
    public decimal TaxPercent { get; init; }
    public int SortOrder { get; init; }
}

public sealed class TerminateContractDto
{
    public string? Reason { get; init; }
}
