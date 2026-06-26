using Nexus.ApiContracts.Dtos;
using Nexus.EventContracts.Crm;
using Nexus.Services.Crm.Contracts.Numbering;
using Nexus.Services.Crm.Contracts.Quotations;
using Nexus.Services.Crm.Domain;
using Nexus.Services.Crm.Domain.Customers;
using Nexus.Services.Crm.Domain.Enums;
using Nexus.Services.Crm.Domain.Quotations;
using Nexus.SharedKernel.Context;
using Nexus.SharedKernel.Events;
using Nexus.SharedKernel.Exceptions;

namespace Nexus.Services.Crm.Application.Quotations;

public sealed class QuotationAppService : CrmAppServiceBase, IQuotationAppService
{
    private readonly IQuotationRepository _quotationRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEventBus _eventBus;
    private readonly INumberingClient _numberingClient;

    public QuotationAppService(
        IQuotationRepository quotationRepository,
        ICustomerRepository customerRepository,
        IEventBus eventBus,
        INumberingClient numberingClient,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        ICorrelationContext correlationContext)
        : base(currentTenant, currentUser, correlationContext)
    {
        _quotationRepository = quotationRepository;
        _customerRepository = customerRepository;
        _eventBus = eventBus;
        _numberingClient = numberingClient;
    }

    public async Task<PagedResultDto<QuotationDto>> GetListAsync(GetQuotationsInput input, CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        var status = input.Status?.ToString();

        var items = await _quotationRepository.GetListByTenantAsync(
            tenantId,
            input.Search,
            status,
            input.CustomerId,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        return new PagedResultDto<QuotationDto>
        {
            TotalCount = await _quotationRepository.GetCountByTenantAsync(tenantId, input.Search, status, input.CustomerId, cancellationToken),
            Items = items.Select(MapToDto).ToArray()
        };
    }

    public async Task<QuotationDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var quotation = await _quotationRepository.GetWithLinesAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Quotation with id '{id}' was not found.");

        EnsureTenantAccess(quotation);
        return MapToDto(quotation);
    }

    public async Task<QuotationDto> CreateAsync(CreateQuotationDto input, CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        await EnsureCustomerExistsAsync(tenantId, input.CustomerId, cancellationToken);

        var quotationNo = string.IsNullOrWhiteSpace(input.QuotationNo) || input.QuotationNo.Equals("AUTO", StringComparison.OrdinalIgnoreCase)
            ? await _numberingClient.GetNextNumberAsync(tenantId, "CRM", "Quotation", "QT-", cancellationToken)
            : input.QuotationNo;

        if (await _quotationRepository.FindByNoAsync(tenantId, quotationNo, cancellationToken) is not null)
        {
            throw new NexusBusinessException(CrmErrorCodes.QuotationAlreadyExists, "Quotation number already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var quotation = new Quotation(
            Guid.NewGuid(),
            tenantId,
            input.CustomerId,
            quotationNo,
            input.OpportunityId,
            input.ContactId,
            input.Subject,
            input.OwnerId,
            CurrentUser.Id,
            now);

        var lines = MapLines(tenantId, quotation.Id, input.Lines);
        quotation.SetLines(lines, CurrentUser.Id, now);

        await _quotationRepository.InsertAsync(quotation, cancellationToken);
        return MapToDto(quotation);
    }

    public async Task<QuotationDto> UpdateAsync(Guid id, UpdateQuotationDto input, CancellationToken cancellationToken = default)
    {
        var quotation = await _quotationRepository.GetWithLinesAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Quotation with id '{id}' was not found.");

        EnsureTenantAccess(quotation);

        var tenantId = GetRequiredTenantId();
        await EnsureCustomerExistsAsync(tenantId, input.CustomerId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        quotation.UpdateHeader(
            input.CustomerId,
            input.OpportunityId,
            input.ContactId,
            input.Subject,
            input.Description,
            input.DiscountAmount,
            input.DiscountPercent,
            input.ValidUntil,
            input.Notes,
            input.Terms,
            input.OwnerId,
            CurrentUser.Id,
            now);

        var lines = MapLines(tenantId, quotation.Id, input.Lines);
        quotation.SetLines(lines, CurrentUser.Id, now);

        await _quotationRepository.UpdateAsync(quotation, cancellationToken);
        return MapToDto(quotation);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var quotation = await _quotationRepository.FindAsync(id, cancellationToken);
        if (quotation is null)
        {
            return;
        }

        EnsureTenantAccess(quotation);
        await _quotationRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<QuotationDto> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var quotation = await _quotationRepository.GetWithLinesAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Quotation with id '{id}' was not found.");

        EnsureTenantAccess(quotation);

        if (quotation.Status is not (QuotationStatus.Draft or QuotationStatus.Sent))
        {
            throw new NexusBusinessException(CrmErrorCodes.InvalidStatusTransition, "Quotation cannot be approved in its current status.");
        }

        var now = DateTimeOffset.UtcNow;
        quotation.Approve(CurrentUser.Id, now);
        await _quotationRepository.UpdateAsync(quotation, cancellationToken);

        await _eventBus.PublishAsync(new QuotationApprovedIntegrationEvent(
            Guid.NewGuid(),
            quotation.TenantId,
            now,
            ServiceName,
            CorrelationContext.CorrelationId,
            quotation.Id,
            quotation.QuotationNo,
            quotation.CustomerId,
            quotation.TotalAmount), cancellationToken);

        return MapToDto(quotation);
    }

    public async Task<QuotationDto> RejectAsync(Guid id, RejectQuotationDto input, CancellationToken cancellationToken = default)
    {
        var quotation = await _quotationRepository.GetWithLinesAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Quotation with id '{id}' was not found.");

        EnsureTenantAccess(quotation);

        if (quotation.Status is not (QuotationStatus.Draft or QuotationStatus.Sent))
        {
            throw new NexusBusinessException(CrmErrorCodes.InvalidStatusTransition, "Quotation cannot be rejected in its current status.");
        }

        var now = DateTimeOffset.UtcNow;
        quotation.Reject(input.Reason, CurrentUser.Id, now);
        await _quotationRepository.UpdateAsync(quotation, cancellationToken);
        return MapToDto(quotation);
    }

    public async Task<QuotationDto> SendAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var quotation = await _quotationRepository.GetWithLinesAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Quotation with id '{id}' was not found.");

        EnsureTenantAccess(quotation);

        if (quotation.Status != QuotationStatus.Draft)
        {
            throw new NexusBusinessException(CrmErrorCodes.InvalidStatusTransition, "Only draft quotations can be sent.");
        }

        var now = DateTimeOffset.UtcNow;
        quotation.Send(CurrentUser.Id, now);
        await _quotationRepository.UpdateAsync(quotation, cancellationToken);
        return MapToDto(quotation);
    }

    private async Task EnsureCustomerExistsAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.FindAsync(customerId, cancellationToken);
        if (customer is null || customer.TenantId != tenantId)
        {
            throw new KeyNotFoundException($"Customer with id '{customerId}' was not found.");
        }
    }

    private static IReadOnlyList<QuotationLine> MapLines(Guid tenantId, Guid quotationId, IReadOnlyList<CreateQuotationLineDto> lines)
    {
        return lines
            .Select(line => new QuotationLine(
                Guid.NewGuid(),
                tenantId,
                quotationId,
                line.LineNo,
                line.ProductCode,
                line.ProductName,
                line.Description,
                line.Quantity,
                line.Unit,
                line.UnitPrice,
                line.DiscountPercent,
                line.TaxPercent,
                line.SortOrder))
            .ToArray();
    }

    private static QuotationDto MapToDto(Quotation quotation)
    {
        return new QuotationDto
        {
            Id = quotation.Id,
            TenantId = quotation.TenantId,
            CustomerId = quotation.CustomerId,
            OpportunityId = quotation.OpportunityId,
            ContactId = quotation.ContactId,
            QuotationNo = quotation.QuotationNo,
            Subject = quotation.Subject,
            Description = quotation.Description,
            Subtotal = quotation.Subtotal,
            DiscountAmount = quotation.DiscountAmount,
            DiscountPercent = quotation.DiscountPercent,
            TaxAmount = quotation.TaxAmount,
            TotalAmount = quotation.TotalAmount,
            Currency = quotation.Currency,
            ValidUntil = quotation.ValidUntil,
            Status = quotation.Status,
            ApprovedAt = quotation.ApprovedAt,
            ApprovedBy = quotation.ApprovedBy,
            RejectedAt = quotation.RejectedAt,
            RejectionReason = quotation.RejectionReason,
            Notes = quotation.Notes,
            Terms = quotation.Terms,
            OwnerId = quotation.OwnerId,
            Lines = quotation.Lines.Select(MapLineToDto).ToArray(),
            CreationTime = quotation.CreationTime,
            CreatorId = quotation.CreatorId,
            LastModificationTime = quotation.LastModificationTime,
            LastModifierId = quotation.LastModifierId,
            ConcurrencyStamp = quotation.ConcurrencyStamp
        };
    }

    private static QuotationLineDto MapLineToDto(QuotationLine line)
    {
        return new QuotationLineDto
        {
            Id = line.Id,
            TenantId = line.TenantId,
            QuotationId = line.QuotationId,
            LineNo = line.LineNo,
            ProductCode = line.ProductCode,
            ProductName = line.ProductName,
            Description = line.Description,
            Quantity = line.Quantity,
            Unit = line.Unit,
            UnitPrice = line.UnitPrice,
            DiscountPercent = line.DiscountPercent,
            TaxPercent = line.TaxPercent,
            LineTotal = line.LineTotal,
            SortOrder = line.SortOrder
        };
    }
}
