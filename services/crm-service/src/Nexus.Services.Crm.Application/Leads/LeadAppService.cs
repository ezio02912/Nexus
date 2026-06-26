using Nexus.ApiContracts.Dtos;
using Nexus.EventContracts.Crm;
using Nexus.Services.Crm.Contracts.Leads;
using Nexus.Services.Crm.Domain;
using Nexus.Services.Crm.Domain.Customers;
using Nexus.Services.Crm.Domain.Enums;
using Nexus.Services.Crm.Domain.Leads;
using Nexus.Services.Crm.Domain.Opportunities;
using Nexus.SharedKernel.Context;
using Nexus.SharedKernel.Events;
using Nexus.SharedKernel.Exceptions;

namespace Nexus.Services.Crm.Application.Leads;

public sealed class LeadAppService : CrmAppServiceBase, ILeadAppService
{
    private readonly ILeadRepository _leadRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly IEventBus? _eventBus;

    public LeadAppService(
        ILeadRepository leadRepository,
        ICustomerRepository customerRepository,
        IOpportunityRepository opportunityRepository,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        ICorrelationContext correlationContext,
        IEventBus? eventBus = null)
        : base(currentTenant, currentUser, correlationContext)
    {
        _leadRepository = leadRepository;
        _customerRepository = customerRepository;
        _opportunityRepository = opportunityRepository;
        _eventBus = eventBus;
    }

    public async Task<PagedResultDto<LeadDto>> GetListAsync(GetLeadsInput input, CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        var status = input.Status?.ToString();

        var items = await _leadRepository.GetListByTenantAsync(
            tenantId,
            input.Search,
            status,
            input.OwnerId,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        return new PagedResultDto<LeadDto>
        {
            TotalCount = await _leadRepository.GetCountByTenantAsync(tenantId, input.Search, status, input.OwnerId, cancellationToken),
            Items = items.Select(MapToDto).ToArray()
        };
    }

    public async Task<LeadDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetAsync(id, cancellationToken);
        EnsureTenantAccess(lead);
        return MapToDto(lead);
    }

    public async Task<LeadDto> CreateAsync(CreateLeadDto input, CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        var now = DateTimeOffset.UtcNow;

        var lead = new Lead(
            Guid.NewGuid(),
            tenantId,
            input.FullName,
            input.CompanyName,
            input.Title,
            input.Email,
            input.Phone,
            input.Source,
            input.OwnerId,
            CurrentUser.Id,
            now);

        await _leadRepository.InsertAsync(lead, cancellationToken);
        return MapToDto(lead);
    }

    public async Task<LeadDto> UpdateAsync(Guid id, UpdateLeadDto input, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetAsync(id, cancellationToken);
        EnsureTenantAccess(lead);

        var now = DateTimeOffset.UtcNow;
        lead.Update(
            input.FullName,
            input.CompanyName,
            input.Title,
            input.Email,
            input.Phone,
            input.Source,
            input.LeadScore,
            input.Rating,
            input.Status,
            input.OwnerId,
            input.Description,
            input.Address,
            input.City,
            input.Country,
            input.LostReason,
            CurrentUser.Id,
            now);

        await _leadRepository.UpdateAsync(lead, cancellationToken);
        return MapToDto(lead);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.FindAsync(id, cancellationToken);
        if (lead is null)
        {
            return;
        }

        EnsureTenantAccess(lead);
        await _leadRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<ConvertLeadResultDto> ConvertAsync(Guid id, ConvertLeadDto input, CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        var lead = await _leadRepository.GetAsync(id, cancellationToken);
        EnsureTenantAccess(lead);

        if (lead.Status == LeadStatus.Converted)
        {
            throw new NexusBusinessException(CrmErrorCodes.LeadAlreadyConverted, "Lead is already converted.");
        }

        var customerCode = string.IsNullOrWhiteSpace(input.CustomerCode)
            ? GenerateCustomerCode(lead)
            : input.CustomerCode;

        if (await _customerRepository.FindByCodeAsync(tenantId, customerCode, cancellationToken) is not null)
        {
            throw new NexusBusinessException(CrmErrorCodes.CustomerAlreadyExists, "Customer code already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var customerName = string.IsNullOrWhiteSpace(lead.CompanyName) ? lead.FullName : lead.CompanyName;

        var customer = new Customer(
            Guid.NewGuid(),
            tenantId,
            customerCode,
            customerName,
            input.CustomerType,
            lead.Email,
            lead.Phone,
            CurrentUser.Id,
            now);

        customer.UpdateProfile(
            customerName,
            input.CustomerType,
            lead.Email,
            lead.Phone,
            null,
            null,
            null,
            lead.Address,
            null,
            lead.City,
            null,
            null,
            lead.Country,
            input.OwnerId ?? lead.OwnerId,
            lead.Description,
            lead.Rating,
            lead.Source,
            CustomerStatus.Prospect,
            CurrentUser.Id,
            now);

        var opportunity = new Opportunity(
            Guid.NewGuid(),
            tenantId,
            customer.Id,
            lead.Id,
            input.OpportunityName,
            input.OpportunityAmount,
            input.ExpectedCloseDate,
            input.OwnerId ?? lead.OwnerId,
            CurrentUser.Id,
            now);

        lead.MarkConverted(customer.Id, opportunity.Id, CurrentUser.Id, now);

        await _customerRepository.InsertAsync(customer, cancellationToken);
        await _opportunityRepository.InsertAsync(opportunity, cancellationToken);
        await _leadRepository.UpdateAsync(lead, cancellationToken);

        if (_eventBus is not null)
        {
            await _eventBus.PublishAsync(new LeadConvertedIntegrationEvent(
                Guid.NewGuid(),
                tenantId,
                now,
                ServiceName,
                CorrelationContext.CorrelationId,
                lead.Id,
                customer.Id,
                opportunity.Id), cancellationToken);
        }

        return new ConvertLeadResultDto
        {
            LeadId = lead.Id,
            CustomerId = customer.Id,
            OpportunityId = opportunity.Id,
            Lead = MapToDto(lead)
        };
    }

    private static string GenerateCustomerCode(Lead lead)
    {
        var source = string.IsNullOrWhiteSpace(lead.CompanyName) ? lead.FullName : lead.CompanyName;
        var normalized = new string(source
            .Trim()
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .Take(12)
            .ToArray());

        if (string.IsNullOrWhiteSpace(normalized))
        {
            normalized = "LEAD";
        }

        return $"{normalized}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
    }

    private static LeadDto MapToDto(Lead lead)
    {
        return new LeadDto
        {
            Id = lead.Id,
            TenantId = lead.TenantId,
            FullName = lead.FullName,
            CompanyName = lead.CompanyName,
            Title = lead.Title,
            Email = lead.Email,
            Phone = lead.Phone,
            Source = lead.Source,
            Status = lead.Status,
            LeadScore = lead.LeadScore,
            Rating = lead.Rating,
            OwnerId = lead.OwnerId,
            AssignedAt = lead.AssignedAt,
            ConvertedCustomerId = lead.ConvertedCustomerId,
            ConvertedOpportunityId = lead.ConvertedOpportunityId,
            ConvertedAt = lead.ConvertedAt,
            LostReason = lead.LostReason,
            Description = lead.Description,
            Address = lead.Address,
            City = lead.City,
            Country = lead.Country,
            CreationTime = lead.CreationTime,
            CreatorId = lead.CreatorId,
            LastModificationTime = lead.LastModificationTime,
            LastModifierId = lead.LastModifierId,
            ConcurrencyStamp = lead.ConcurrencyStamp
        };
    }
}
