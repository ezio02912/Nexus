using Nexus.Services.Crm.Domain.Enums;
using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Repositories;
using Nexus.SharedKernel.Validation;

namespace Nexus.Services.Crm.Domain.Opportunities;

public static class OpportunityConsts
{
    public const int NameMaxLength = 256;
    public const int CurrencyMaxLength = 3;
    public const int SourceMaxLength = 128;
    public const int CompetitorMaxLength = 256;
    public const int NextStepMaxLength = 512;
}

public sealed class Opportunity : FullAuditedAggregateRoot<Guid>
{
    private Opportunity()
    {
        Name = string.Empty;
        Currency = "VND";
    }

    public Opportunity(
        Guid id,
        Guid tenantId,
        Guid? customerId,
        Guid? leadId,
        string name,
        decimal amount,
        DateOnly? expectedCloseDate,
        Guid? ownerId,
        Guid? creatorId,
        DateTimeOffset now)
    {
        Id = id;
        CustomerId = customerId;
        LeadId = leadId;
        Name = Check.Length(Check.NotNullOrWhiteSpace(name, nameof(name)), nameof(name), OpportunityConsts.NameMaxLength);
        Amount = amount;
        ExpectedCloseDate = expectedCloseDate;
        OwnerId = ownerId;
        Stage = OpportunityStage.Prospecting;
        Probability = 10;
        Currency = "VND";
        SetCreationAudit(tenantId, creatorId, now);
    }

    public Guid? CustomerId { get; private set; }
    public Guid? LeadId { get; private set; }
    public Guid? ContactId { get; private set; }
    public string Name { get; private set; }
    public OpportunityStage Stage { get; private set; }
    public decimal Amount { get; private set; }
    public int Probability { get; private set; }
    public string Currency { get; private set; }
    public DateOnly? ExpectedCloseDate { get; private set; }
    public DateOnly? ActualCloseDate { get; private set; }
    public string? CloseReason { get; private set; }
    public string? LostReason { get; private set; }
    public string? Description { get; private set; }
    public string? NextStep { get; private set; }
    public DateOnly? NextStepDate { get; private set; }
    public string? Source { get; private set; }
    public string? Competitor { get; private set; }
    public Guid? OwnerId { get; private set; }

    public void Update(
        Guid? customerId,
        Guid? contactId,
        string name,
        decimal amount,
        int probability,
        string currency,
        DateOnly? expectedCloseDate,
        string? description,
        string? nextStep,
        DateOnly? nextStepDate,
        string? source,
        string? competitor,
        Guid? ownerId,
        Guid? modifierId,
        DateTimeOffset now)
    {
        CustomerId = customerId;
        ContactId = contactId;
        Name = Check.Length(Check.NotNullOrWhiteSpace(name, nameof(name)), nameof(name), OpportunityConsts.NameMaxLength);
        Amount = amount;
        Probability = Math.Clamp(probability, 0, 100);
        Currency = Check.Length(Check.NotNullOrWhiteSpace(currency, nameof(currency)), nameof(currency), OpportunityConsts.CurrencyMaxLength);
        ExpectedCloseDate = expectedCloseDate;
        Description = description?.Trim();
        NextStep = NormalizeOptional(nextStep, OpportunityConsts.NextStepMaxLength);
        NextStepDate = nextStepDate;
        Source = NormalizeOptional(source, OpportunityConsts.SourceMaxLength);
        Competitor = NormalizeOptional(competitor, OpportunityConsts.CompetitorMaxLength);
        OwnerId = ownerId;
        SetModificationAudit(modifierId, now);
    }

    public void ChangeStage(OpportunityStage stage, int? probability, string? closeReason, string? lostReason, Guid? modifierId, DateTimeOffset now)
    {
        Stage = stage;
        if (probability.HasValue)
        {
            Probability = Math.Clamp(probability.Value, 0, 100);
        }

        if (stage is OpportunityStage.ClosedWon or OpportunityStage.ClosedLost)
        {
            ActualCloseDate = DateOnly.FromDateTime(now.DateTime);
        }

        CloseReason = closeReason?.Trim();
        LostReason = lostReason?.Trim();
        SetModificationAudit(modifierId, now);
    }

    private static string? NormalizeOptional(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value) ? null : Check.Length(value.Trim(), nameof(value), maxLength);
}

public interface IOpportunityRepository : IRepository<Opportunity, Guid>
{
    Task<IReadOnlyList<Opportunity>> GetListByTenantAsync(Guid tenantId, string? search, string? stage, Guid? customerId, Guid? ownerId, int skipCount, int maxResultCount, CancellationToken cancellationToken = default);
    Task<long> GetCountByTenantAsync(Guid tenantId, string? search, string? stage, Guid? customerId, Guid? ownerId, CancellationToken cancellationToken = default);
    Task<decimal> GetOpenPipelineValueAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
