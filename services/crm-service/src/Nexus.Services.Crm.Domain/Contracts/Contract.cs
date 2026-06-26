using Nexus.Services.Crm.Domain.Enums;
using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Repositories;
using Nexus.SharedKernel.Validation;

namespace Nexus.Services.Crm.Domain.Contracts;

public static class ContractConsts
{
    public const int ContractNoMaxLength = 64;
    public const int TitleMaxLength = 256;
    public const int CurrencyMaxLength = 3;
    public const int ProductCodeMaxLength = 64;
    public const int ProductNameMaxLength = 256;
    public const int UnitMaxLength = 32;
}

public sealed class Contract : FullAuditedAggregateRoot<Guid>
{
    private readonly List<ContractLine> _lines = [];

    private Contract()
    {
        ContractNo = string.Empty;
        Title = string.Empty;
        Currency = "VND";
    }

    public Contract(
        Guid id,
        Guid tenantId,
        Guid customerId,
        string contractNo,
        string title,
        Guid? quotationId,
        Guid? opportunityId,
        Guid? contactId,
        decimal contractValue,
        Guid? ownerId,
        Guid? creatorId,
        DateTimeOffset now)
    {
        Id = id;
        CustomerId = customerId;
        ContractNo = Check.Length(Check.NotNullOrWhiteSpace(contractNo, nameof(contractNo)), nameof(contractNo), ContractConsts.ContractNoMaxLength).ToUpperInvariant();
        Title = Check.Length(Check.NotNullOrWhiteSpace(title, nameof(title)), nameof(title), ContractConsts.TitleMaxLength);
        QuotationId = quotationId;
        OpportunityId = opportunityId;
        ContactId = contactId;
        ContractValue = contractValue;
        OwnerId = ownerId;
        Status = ContractStatus.Draft;
        Currency = "VND";
        SetCreationAudit(tenantId, creatorId, now);
    }

    public Guid CustomerId { get; private set; }
    public Guid? QuotationId { get; private set; }
    public Guid? OpportunityId { get; private set; }
    public Guid? ContactId { get; private set; }
    public string ContractNo { get; private set; }
    public string Title { get; private set; }
    public decimal ContractValue { get; private set; }
    public string Currency { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public DateOnly? RenewalDate { get; private set; }
    public ContractStatus Status { get; private set; }
    public DateTimeOffset? SignedAt { get; private set; }
    public Guid? SignedBy { get; private set; }
    public string? TerminationReason { get; private set; }
    public string? PaymentTerms { get; private set; }
    public string? Notes { get; private set; }
    public string? Terms { get; private set; }
    public Guid? FileId { get; private set; }
    public Guid? OwnerId { get; private set; }
    public IReadOnlyCollection<ContractLine> Lines => _lines.AsReadOnly();

    public void UpdateHeader(
        Guid customerId,
        Guid? quotationId,
        Guid? opportunityId,
        Guid? contactId,
        string title,
        decimal contractValue,
        string currency,
        DateOnly? startDate,
        DateOnly? endDate,
        DateOnly? renewalDate,
        string? paymentTerms,
        string? notes,
        string? terms,
        Guid? fileId,
        Guid? ownerId,
        Guid? modifierId,
        DateTimeOffset now)
    {
        CustomerId = customerId;
        QuotationId = quotationId;
        OpportunityId = opportunityId;
        ContactId = contactId;
        Title = Check.Length(Check.NotNullOrWhiteSpace(title, nameof(title)), nameof(title), ContractConsts.TitleMaxLength);
        ContractValue = contractValue;
        Currency = Check.Length(Check.NotNullOrWhiteSpace(currency, nameof(currency)), nameof(currency), ContractConsts.CurrencyMaxLength);
        StartDate = startDate;
        EndDate = endDate;
        RenewalDate = renewalDate;
        PaymentTerms = paymentTerms?.Trim();
        Notes = notes?.Trim();
        Terms = terms?.Trim();
        FileId = fileId;
        OwnerId = ownerId;
        SetModificationAudit(modifierId, now);
    }

    public void SetLines(IEnumerable<ContractLine> lines, Guid? modifierId, DateTimeOffset now)
    {
        _lines.Clear();
        _lines.AddRange(lines);
        ContractValue = _lines.Sum(x => x.LineTotal);
        SetModificationAudit(modifierId, now);
    }

    public void Sign(Guid? signerId, DateTimeOffset now)
    {
        Status = ContractStatus.Signed;
        SignedAt = now;
        SignedBy = signerId;
        SetModificationAudit(signerId, now);
    }

    public void Terminate(string? reason, Guid? modifierId, DateTimeOffset now)
    {
        Status = ContractStatus.Terminated;
        TerminationReason = reason?.Trim();
        SetModificationAudit(modifierId, now);
    }

    public void Activate(Guid? modifierId, DateTimeOffset now)
    {
        Status = ContractStatus.Active;
        SetModificationAudit(modifierId, now);
    }
}

public sealed class ContractLine : NexusEntity<Guid>
{
    private ContractLine()
    {
        ProductCode = string.Empty;
        ProductName = string.Empty;
        Unit = "EA";
    }

    public ContractLine(
        Guid id,
        Guid tenantId,
        Guid contractId,
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
        ContractId = contractId;
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
    public Guid ContractId { get; private set; }
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

    private void RecalculateLineTotal()
    {
        var gross = Quantity * UnitPrice;
        LineTotal = gross - gross * DiscountPercent / 100m;
    }
}

public interface IContractRepository : IRepository<Contract, Guid>
{
    Task<Contract?> FindByNoAsync(Guid tenantId, string contractNo, CancellationToken cancellationToken = default);
    Task<Contract?> GetWithLinesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Contract>> GetListByTenantAsync(Guid tenantId, string? search, string? status, Guid? customerId, int skipCount, int maxResultCount, CancellationToken cancellationToken = default);
    Task<long> GetCountByTenantAsync(Guid tenantId, string? search, string? status, Guid? customerId, CancellationToken cancellationToken = default);
    Task<long> GetExpiringCountAsync(Guid tenantId, int withinDays, CancellationToken cancellationToken = default);
}
