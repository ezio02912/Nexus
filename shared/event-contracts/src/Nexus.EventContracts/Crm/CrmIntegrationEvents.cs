using Nexus.SharedKernel.Events;

namespace Nexus.EventContracts.Crm;

public sealed record CustomerCreatedIntegrationEvent(
    Guid EventId,
    Guid? TenantId,
    DateTimeOffset OccurredAt,
    string SourceService,
    string? CorrelationId,
    Guid CustomerId,
    string Code,
    string Name) : IIntegrationEvent
{
    public string EventName => nameof(CustomerCreatedIntegrationEvent);
}

public sealed record LeadConvertedIntegrationEvent(
    Guid EventId,
    Guid? TenantId,
    DateTimeOffset OccurredAt,
    string SourceService,
    string? CorrelationId,
    Guid LeadId,
    Guid CustomerId,
    Guid OpportunityId) : IIntegrationEvent
{
    public string EventName => nameof(LeadConvertedIntegrationEvent);
}

public sealed record OpportunityStageChangedIntegrationEvent(
    Guid EventId,
    Guid? TenantId,
    DateTimeOffset OccurredAt,
    string SourceService,
    string? CorrelationId,
    Guid OpportunityId,
    string Stage) : IIntegrationEvent
{
    public string EventName => nameof(OpportunityStageChangedIntegrationEvent);
}

public sealed record QuotationApprovedIntegrationEvent(
    Guid EventId,
    Guid? TenantId,
    DateTimeOffset OccurredAt,
    string SourceService,
    string? CorrelationId,
    Guid QuotationId,
    string QuotationNo,
    Guid CustomerId,
    decimal TotalAmount) : IIntegrationEvent
{
    public string EventName => nameof(QuotationApprovedIntegrationEvent);
}

public sealed record QuotationRejectedIntegrationEvent(
    Guid EventId,
    Guid? TenantId,
    DateTimeOffset OccurredAt,
    string SourceService,
    string? CorrelationId,
    Guid QuotationId,
    string QuotationNo) : IIntegrationEvent
{
    public string EventName => nameof(QuotationRejectedIntegrationEvent);
}

public sealed record ContractSignedIntegrationEvent(
    Guid EventId,
    Guid? TenantId,
    DateTimeOffset OccurredAt,
    string SourceService,
    string? CorrelationId,
    Guid ContractId,
    string ContractNo,
    Guid CustomerId,
    decimal ContractValue) : IIntegrationEvent
{
    public string EventName => nameof(ContractSignedIntegrationEvent);
}
