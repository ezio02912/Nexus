using Nexus.ApiContracts.Dtos;
using Nexus.Services.Crm.Contracts.Opportunities;
using Nexus.Services.Crm.Domain.Customers;
using Nexus.Services.Crm.Domain.Opportunities;
using Nexus.SharedKernel.Context;

namespace Nexus.Services.Crm.Application.Opportunities;

public sealed class OpportunityAppService : CrmAppServiceBase, IOpportunityAppService
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ICustomerRepository _customerRepository;

    public OpportunityAppService(
        IOpportunityRepository opportunityRepository,
        ICustomerRepository customerRepository,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        ICorrelationContext correlationContext)
        : base(currentTenant, currentUser, correlationContext)
    {
        _opportunityRepository = opportunityRepository;
        _customerRepository = customerRepository;
    }

    public async Task<PagedResultDto<OpportunityDto>> GetListAsync(GetOpportunitiesInput input, CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        var stage = input.Stage?.ToString();

        var items = await _opportunityRepository.GetListByTenantAsync(
            tenantId,
            input.Search,
            stage,
            input.CustomerId,
            input.OwnerId,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        return new PagedResultDto<OpportunityDto>
        {
            TotalCount = await _opportunityRepository.GetCountByTenantAsync(tenantId, input.Search, stage, input.CustomerId, input.OwnerId, cancellationToken),
            Items = items.Select(MapToDto).ToArray()
        };
    }

    public async Task<OpportunityDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var opportunity = await _opportunityRepository.GetAsync(id, cancellationToken);
        EnsureTenantAccess(opportunity);
        return MapToDto(opportunity);
    }

    public async Task<OpportunityDto> CreateAsync(CreateOpportunityDto input, CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        await EnsureCustomerExistsIfProvidedAsync(tenantId, input.CustomerId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var opportunity = new Opportunity(
            Guid.NewGuid(),
            tenantId,
            input.CustomerId,
            input.LeadId,
            input.Name,
            input.Amount,
            input.ExpectedCloseDate,
            input.OwnerId,
            CurrentUser.Id,
            now);

        await _opportunityRepository.InsertAsync(opportunity, cancellationToken);
        return MapToDto(opportunity);
    }

    public async Task<OpportunityDto> UpdateAsync(Guid id, UpdateOpportunityDto input, CancellationToken cancellationToken = default)
    {
        var opportunity = await _opportunityRepository.GetAsync(id, cancellationToken);
        EnsureTenantAccess(opportunity);

        var tenantId = GetRequiredTenantId();
        await EnsureCustomerExistsIfProvidedAsync(tenantId, input.CustomerId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        opportunity.Update(
            input.CustomerId,
            input.ContactId,
            input.Name,
            input.Amount,
            input.Probability,
            input.Currency,
            input.ExpectedCloseDate,
            input.Description,
            input.NextStep,
            input.NextStepDate,
            input.Source,
            input.Competitor,
            input.OwnerId,
            CurrentUser.Id,
            now);

        await _opportunityRepository.UpdateAsync(opportunity, cancellationToken);
        return MapToDto(opportunity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var opportunity = await _opportunityRepository.FindAsync(id, cancellationToken);
        if (opportunity is null)
        {
            return;
        }

        EnsureTenantAccess(opportunity);
        await _opportunityRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<OpportunityDto> ChangeStageAsync(Guid id, ChangeOpportunityStageDto input, CancellationToken cancellationToken = default)
    {
        var opportunity = await _opportunityRepository.GetAsync(id, cancellationToken);
        EnsureTenantAccess(opportunity);

        var now = DateTimeOffset.UtcNow;
        opportunity.ChangeStage(
            input.Stage,
            input.Probability,
            input.CloseReason,
            input.LostReason,
            CurrentUser.Id,
            now);

        await _opportunityRepository.UpdateAsync(opportunity, cancellationToken);
        return MapToDto(opportunity);
    }

    private async Task EnsureCustomerExistsIfProvidedAsync(Guid tenantId, Guid? customerId, CancellationToken cancellationToken)
    {
        if (!customerId.HasValue)
        {
            return;
        }

        var customer = await _customerRepository.FindAsync(customerId.Value, cancellationToken);
        if (customer is null || customer.TenantId != tenantId)
        {
            throw new KeyNotFoundException($"Customer with id '{customerId.Value}' was not found.");
        }
    }

    private static OpportunityDto MapToDto(Opportunity opportunity)
    {
        return new OpportunityDto
        {
            Id = opportunity.Id,
            TenantId = opportunity.TenantId,
            CustomerId = opportunity.CustomerId,
            LeadId = opportunity.LeadId,
            ContactId = opportunity.ContactId,
            Name = opportunity.Name,
            Stage = opportunity.Stage,
            Amount = opportunity.Amount,
            Probability = opportunity.Probability,
            Currency = opportunity.Currency,
            ExpectedCloseDate = opportunity.ExpectedCloseDate,
            ActualCloseDate = opportunity.ActualCloseDate,
            CloseReason = opportunity.CloseReason,
            LostReason = opportunity.LostReason,
            Description = opportunity.Description,
            NextStep = opportunity.NextStep,
            NextStepDate = opportunity.NextStepDate,
            Source = opportunity.Source,
            Competitor = opportunity.Competitor,
            OwnerId = opportunity.OwnerId,
            CreationTime = opportunity.CreationTime,
            CreatorId = opportunity.CreatorId,
            LastModificationTime = opportunity.LastModificationTime,
            LastModifierId = opportunity.LastModifierId,
            ConcurrencyStamp = opportunity.ConcurrencyStamp
        };
    }
}
