using Nexus.Services.Crm.Domain.Enums;
using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Repositories;
using Nexus.SharedKernel.Validation;

namespace Nexus.Services.Crm.Domain.Quotations;

public static class QuotationConsts
{
    public const int QuotationNoMaxLength = 64;
    public const int SubjectMaxLength = 256;
    public const int CurrencyMaxLength = 3;
    public const int ProductCodeMaxLength = 64;
    public const int ProductNameMaxLength = 256;
    public const int UnitMaxLength = 32;
}

public sealed class Quotation : FullAuditedAggregateRoot<Guid>
{
    private readonly List<QuotationLine> _lines = [];

    private Quotation()
    {
        QuotationNo = string.Empty;
        Currency = "VND";
    }

    public Quotation(
        Guid id,
        Guid tenantId,
        Guid customerId,
        string quotationNo,
        Guid? opportunityId,
        Guid? contactId,
        string? subject,
        Guid? ownerId,
        Guid? creatorId,
        DateTimeOffset now)
    {
        Id = id;
        CustomerId = customerId;
        QuotationNo = Check.Length(Check.NotNullOrWhiteSpace(quotationNo, nameof(quotationNo)), nameof(quotationNo), QuotationConsts.QuotationNoMaxLength).ToUpperInvariant();
        OpportunityId = opportunityId;
        ContactId = contactId;
        Subject = NormalizeOptional(subject, QuotationConsts.SubjectMaxLength);
        OwnerId = ownerId;
        Status = QuotationStatus.Draft;
        Currency = "VND";
        SetCreationAudit(tenantId, creatorId, now);
    }

    public Guid CustomerId { get; private set; }
    public Guid? OpportunityId { get; private set; }
    public Guid? ContactId { get; private set; }
    public string QuotationNo { get; private set; }
    public string? Subject { get; private set; }
    public string? Description { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; }
    public DateOnly? ValidUntil { get; private set; }
    public QuotationStatus Status { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset? RejectedAt { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? Notes { get; private set; }
    public string? Terms { get; private set; }
    public Guid? OwnerId { get; private set; }
    public IReadOnlyCollection<QuotationLine> Lines => _lines.AsReadOnly();

    public void UpdateHeader(
        Guid customerId,
        Guid? opportunityId,
        Guid? contactId,
        string? subject,
        string? description,
        decimal discountAmount,
        decimal discountPercent,
        DateOnly? validUntil,
        string? notes,
        string? terms,
        Guid? ownerId,
        Guid? modifierId,
        DateTimeOffset now)
    {
        CustomerId = customerId;
        OpportunityId = opportunityId;
        ContactId = contactId;
        Subject = NormalizeOptional(subject, QuotationConsts.SubjectMaxLength);
        Description = description?.Trim();
        DiscountAmount = discountAmount;
        DiscountPercent = discountPercent;
        ValidUntil = validUntil;
        Notes = notes?.Trim();
        Terms = terms?.Trim();
        OwnerId = ownerId;
        RecalculateTotals();
        SetModificationAudit(modifierId, now);
    }

    public void SetLines(IEnumerable<QuotationLine> lines, Guid? modifierId, DateTimeOffset now)
    {
        _lines.Clear();
        _lines.AddRange(lines);
        RecalculateTotals();
        SetModificationAudit(modifierId, now);
    }

    public void Approve(Guid? approverId, DateTimeOffset now)
    {
        Status = QuotationStatus.Approved;
        ApprovedAt = now;
        ApprovedBy = approverId;
        RejectedAt = null;
        RejectionReason = null;
        SetModificationAudit(approverId, now);
    }

    public void Reject(string? reason, Guid? rejecterId, DateTimeOffset now)
    {
        Status = QuotationStatus.Rejected;
        RejectedAt = now;
        RejectionReason = reason?.Trim();
        SetModificationAudit(rejecterId, now);
    }

    public void Send(Guid? modifierId, DateTimeOffset now)
    {
        Status = QuotationStatus.Sent;
        SetModificationAudit(modifierId, now);
    }

    public void RecalculateTotals()
    {
        Subtotal = _lines.Sum(x => x.LineTotal);
        var discount = DiscountAmount > 0 ? DiscountAmount : Subtotal * DiscountPercent / 100m;
        TaxAmount = _lines.Sum(x => x.LineTotal * x.TaxPercent / 100m);
        TotalAmount = Subtotal - discount + TaxAmount;
    }

    private static string? NormalizeOptional(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value) ? null : Check.Length(value.Trim(), nameof(value), maxLength);
}

public sealed class QuotationLine : NexusEntity<Guid>
{
    private QuotationLine()
    {
        ProductCode = string.Empty;
        ProductName = string.Empty;
        Unit = "EA";
    }

    public QuotationLine(
        Guid id,
        Guid tenantId,
        Guid quotationId,
        int lineNo,
        string productCode,
        string productName,
        string? description,
        decimal quantity,
        string unit,
        decimal unitPrice,
        decimal discountPercent,
        decimal taxPercent,
        int sortOrder)
    {
        Id = id;
        TenantId = tenantId;
        QuotationId = quotationId;
        LineNo = lineNo;
        ProductCode = Check.NotNullOrWhiteSpace(productCode, nameof(productCode));
        ProductName = Check.NotNullOrWhiteSpace(productName, nameof(productName));
        Description = description?.Trim();
        Quantity = quantity;
        Unit = unit;
        UnitPrice = unitPrice;
        DiscountPercent = discountPercent;
        TaxPercent = taxPercent;
        SortOrder = sortOrder;
        RecalculateLineTotal();
    }

    public Guid TenantId { get; private set; }
    public Guid QuotationId { get; private set; }
    public int LineNo { get; private set; }
    public string ProductCode { get; private set; }
    public string ProductName { get; private set; }
    public string? Description { get; private set; }
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal LineTotal { get; private set; }
    public int SortOrder { get; private set; }

    public void Update(
        int lineNo,
        string productCode,
        string productName,
        string? description,
        decimal quantity,
        string unit,
        decimal unitPrice,
        decimal discountPercent,
        decimal taxPercent,
        int sortOrder)
    {
        LineNo = lineNo;
        ProductCode = Check.NotNullOrWhiteSpace(productCode, nameof(productCode));
        ProductName = Check.NotNullOrWhiteSpace(productName, nameof(productName));
        Description = description?.Trim();
        Quantity = quantity;
        Unit = unit;
        UnitPrice = unitPrice;
        DiscountPercent = discountPercent;
        TaxPercent = taxPercent;
        SortOrder = sortOrder;
        RecalculateLineTotal();
    }

    private void RecalculateLineTotal()
    {
        var gross = Quantity * UnitPrice;
        LineTotal = gross - gross * DiscountPercent / 100m;
    }
}

public interface IQuotationRepository : IRepository<Quotation, Guid>
{
    Task<Quotation?> FindByNoAsync(Guid tenantId, string quotationNo, CancellationToken cancellationToken = default);
    Task<Quotation?> GetWithLinesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Quotation>> GetListByTenantAsync(Guid tenantId, string? search, string? status, Guid? customerId, int skipCount, int maxResultCount, CancellationToken cancellationToken = default);
    Task<long> GetCountByTenantAsync(Guid tenantId, string? search, string? status, Guid? customerId, CancellationToken cancellationToken = default);
    Task<long> GetPendingApprovalCountAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
