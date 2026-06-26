using Nexus.ApiContracts.Dtos;
using Nexus.EventContracts.Crm;
using Nexus.Services.Crm.Contracts.Contracts;
using Nexus.Services.Crm.Contracts.Numbering;
using Nexus.Services.Crm.Domain;
using Nexus.Services.Crm.Domain.Contracts;
using Nexus.Services.Crm.Domain.Customers;
using Nexus.Services.Crm.Domain.Enums;
using Nexus.SharedKernel.Context;
using Nexus.SharedKernel.Events;
using Nexus.SharedKernel.Exceptions;
using DomainContract = Nexus.Services.Crm.Domain.Contracts.Contract;

namespace Nexus.Services.Crm.Application.Contracts;

public sealed class ContractAppService : CrmAppServiceBase, IContractAppService
{
    private readonly IContractRepository _contractRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEventBus _eventBus;
    private readonly INumberingClient _numberingClient;

    public ContractAppService(
        IContractRepository contractRepository,
        ICustomerRepository customerRepository,
        IEventBus eventBus,
        INumberingClient numberingClient,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        ICorrelationContext correlationContext)
        : base(currentTenant, currentUser, correlationContext)
    {
        _contractRepository = contractRepository;
        _customerRepository = customerRepository;
        _eventBus = eventBus;
        _numberingClient = numberingClient;
    }

    public async Task<PagedResultDto<ContractDto>> GetListAsync(GetContractsInput input, CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        var status = input.Status?.ToString();

        var items = await _contractRepository.GetListByTenantAsync(
            tenantId,
            input.Search,
            status,
            input.CustomerId,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        return new PagedResultDto<ContractDto>
        {
            TotalCount = await _contractRepository.GetCountByTenantAsync(tenantId, input.Search, status, input.CustomerId, cancellationToken),
            Items = items.Select(MapToDto).ToArray()
        };
    }

    public async Task<ContractDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetWithLinesAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract with id '{id}' was not found.");

        EnsureTenantAccess(contract);
        return MapToDto(contract);
    }

    public async Task<ContractDto> CreateAsync(CreateContractDto input, CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        await EnsureCustomerExistsAsync(tenantId, input.CustomerId, cancellationToken);

        var contractNo = string.IsNullOrWhiteSpace(input.ContractNo) || input.ContractNo.Equals("AUTO", StringComparison.OrdinalIgnoreCase)
            ? await _numberingClient.GetNextNumberAsync(tenantId, "CRM", "Contract", "CT-", cancellationToken)
            : input.ContractNo;

        if (await _contractRepository.FindByNoAsync(tenantId, contractNo, cancellationToken) is not null)
        {
            throw new NexusBusinessException(CrmErrorCodes.ContractAlreadyExists, "Contract number already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var contract = new DomainContract(
            Guid.NewGuid(),
            tenantId,
            input.CustomerId,
            contractNo,
            input.Title,
            input.QuotationId,
            input.OpportunityId,
            input.ContactId,
            input.ContractValue,
            input.OwnerId,
            CurrentUser.Id,
            now);

        var lines = MapLines(tenantId, contract.Id, input.Lines);
        if (lines.Count > 0)
        {
            contract.SetLines(lines, CurrentUser.Id, now);
        }

        await _contractRepository.InsertAsync(contract, cancellationToken);
        return MapToDto(contract);
    }

    public async Task<ContractDto> UpdateAsync(Guid id, UpdateContractDto input, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetWithLinesAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract with id '{id}' was not found.");

        EnsureTenantAccess(contract);

        var tenantId = GetRequiredTenantId();
        await EnsureCustomerExistsAsync(tenantId, input.CustomerId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        contract.UpdateHeader(
            input.CustomerId,
            input.QuotationId,
            input.OpportunityId,
            input.ContactId,
            input.Title,
            input.ContractValue,
            input.Currency,
            input.StartDate,
            input.EndDate,
            input.RenewalDate,
            input.PaymentTerms,
            input.Notes,
            input.Terms,
            input.FileId,
            input.OwnerId,
            CurrentUser.Id,
            now);

        var lines = MapLines(tenantId, contract.Id, input.Lines);
        contract.SetLines(lines, CurrentUser.Id, now);

        await _contractRepository.UpdateAsync(contract, cancellationToken);
        return MapToDto(contract);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.FindAsync(id, cancellationToken);
        if (contract is null)
        {
            return;
        }

        EnsureTenantAccess(contract);
        await _contractRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<ContractDto> SignAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetWithLinesAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract with id '{id}' was not found.");

        EnsureTenantAccess(contract);

        if (contract.Status is not (ContractStatus.Draft or ContractStatus.PendingSign))
        {
            throw new NexusBusinessException(CrmErrorCodes.InvalidStatusTransition, "Contract cannot be signed in its current status.");
        }

        var now = DateTimeOffset.UtcNow;
        contract.Sign(CurrentUser.Id, now);
        await _contractRepository.UpdateAsync(contract, cancellationToken);

        await _eventBus.PublishAsync(new ContractSignedIntegrationEvent(
            Guid.NewGuid(),
            contract.TenantId,
            now,
            ServiceName,
            CorrelationContext.CorrelationId,
            contract.Id,
            contract.ContractNo,
            contract.CustomerId,
            contract.ContractValue), cancellationToken);

        return MapToDto(contract);
    }

    public async Task<ContractDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetWithLinesAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract with id '{id}' was not found.");

        EnsureTenantAccess(contract);

        if (contract.Status != ContractStatus.Signed)
        {
            throw new NexusBusinessException(CrmErrorCodes.InvalidStatusTransition, "Only signed contracts can be activated.");
        }

        var now = DateTimeOffset.UtcNow;
        contract.Activate(CurrentUser.Id, now);
        await _contractRepository.UpdateAsync(contract, cancellationToken);
        return MapToDto(contract);
    }

    public async Task<ContractDto> TerminateAsync(Guid id, TerminateContractDto input, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetWithLinesAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract with id '{id}' was not found.");

        EnsureTenantAccess(contract);

        if (contract.Status is not (ContractStatus.Signed or ContractStatus.Active))
        {
            throw new NexusBusinessException(CrmErrorCodes.InvalidStatusTransition, "Contract cannot be terminated in its current status.");
        }

        var now = DateTimeOffset.UtcNow;
        contract.Terminate(input.Reason, CurrentUser.Id, now);
        await _contractRepository.UpdateAsync(contract, cancellationToken);
        return MapToDto(contract);
    }

    private async Task EnsureCustomerExistsAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.FindAsync(customerId, cancellationToken);
        if (customer is null || customer.TenantId != tenantId)
        {
            throw new KeyNotFoundException($"Customer with id '{customerId}' was not found.");
        }
    }

    private static IReadOnlyList<ContractLine> MapLines(Guid tenantId, Guid contractId, IReadOnlyList<CreateContractLineDto> lines)
    {
        return lines
            .Select(line => new ContractLine(
                Guid.NewGuid(),
                tenantId,
                contractId,
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

    private static ContractDto MapToDto(DomainContract contract)
    {
        return new ContractDto
        {
            Id = contract.Id,
            TenantId = contract.TenantId,
            CustomerId = contract.CustomerId,
            QuotationId = contract.QuotationId,
            OpportunityId = contract.OpportunityId,
            ContactId = contract.ContactId,
            ContractNo = contract.ContractNo,
            Title = contract.Title,
            ContractValue = contract.ContractValue,
            Currency = contract.Currency,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            RenewalDate = contract.RenewalDate,
            Status = contract.Status,
            SignedAt = contract.SignedAt,
            SignedBy = contract.SignedBy,
            TerminationReason = contract.TerminationReason,
            PaymentTerms = contract.PaymentTerms,
            Notes = contract.Notes,
            Terms = contract.Terms,
            FileId = contract.FileId,
            OwnerId = contract.OwnerId,
            Lines = contract.Lines.Select(MapLineToDto).ToArray(),
            CreationTime = contract.CreationTime,
            CreatorId = contract.CreatorId,
            LastModificationTime = contract.LastModificationTime,
            LastModifierId = contract.LastModifierId,
            ConcurrencyStamp = contract.ConcurrencyStamp
        };
    }

    private static ContractLineDto MapLineToDto(ContractLine line)
    {
        return new ContractLineDto
        {
            Id = line.Id,
            TenantId = line.TenantId,
            ContractId = line.ContractId,
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
