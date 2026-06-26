using Nexus.ApiContracts.Dtos;
using Nexus.Services.Crm.Domain.Enums;

namespace Nexus.Services.Crm.Contracts.Quotations;

public sealed class GetQuotationsInput : PagedAndSortedResultRequestDto
{
    public string? Search { get; init; }
    public QuotationStatus? Status { get; init; }
    public Guid? CustomerId { get; init; }
}

public sealed class CreateQuotationDto
{
    public required Guid CustomerId { get; init; }
    public required string QuotationNo { get; init; }
    public Guid? OpportunityId { get; init; }
    public Guid? ContactId { get; init; }
    public string? Subject { get; init; }
    public Guid? OwnerId { get; init; }
    public IReadOnlyList<CreateQuotationLineDto> Lines { get; init; } = [];
}

public sealed class UpdateQuotationDto
{
    public required Guid CustomerId { get; init; }
    public Guid? OpportunityId { get; init; }
    public Guid? ContactId { get; init; }
    public string? Subject { get; init; }
    public string? Description { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal DiscountPercent { get; init; }
    public DateOnly? ValidUntil { get; init; }
    public string? Notes { get; init; }
    public string? Terms { get; init; }
    public Guid? OwnerId { get; init; }
    public IReadOnlyList<CreateQuotationLineDto> Lines { get; init; } = [];
}

public sealed class QuotationDto
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public Guid CustomerId { get; init; }
    public Guid? OpportunityId { get; init; }
    public Guid? ContactId { get; init; }
    public string QuotationNo { get; init; } = string.Empty;
    public string? Subject { get; init; }
    public string? Description { get; init; }
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal DiscountPercent { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateOnly? ValidUntil { get; init; }
    public QuotationStatus Status { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public Guid? ApprovedBy { get; init; }
    public DateTimeOffset? RejectedAt { get; init; }
    public string? RejectionReason { get; init; }
    public string? Notes { get; init; }
    public string? Terms { get; init; }
    public Guid? OwnerId { get; init; }
    public IReadOnlyList<QuotationLineDto> Lines { get; init; } = [];
    public DateTimeOffset CreationTime { get; init; }
    public Guid? CreatorId { get; init; }
    public DateTimeOffset? LastModificationTime { get; init; }
    public Guid? LastModifierId { get; init; }
    public string ConcurrencyStamp { get; init; } = string.Empty;
}

public sealed class QuotationLineDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid QuotationId { get; init; }
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

public sealed class CreateQuotationLineDto
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

public sealed class RejectQuotationDto
{
    public string? Reason { get; init; }
}
