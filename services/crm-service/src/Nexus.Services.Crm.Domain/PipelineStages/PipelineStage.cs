using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Repositories;
using Nexus.SharedKernel.Validation;

namespace Nexus.Services.Crm.Domain.PipelineStages;

public sealed class PipelineStage : FullAuditedAggregateRoot<Guid>
{
    private PipelineStage()
    {
        Code = string.Empty;
        Name = string.Empty;
        EntityType = string.Empty;
    }

    public PipelineStage(
        Guid id,
        Guid tenantId,
        string entityType,
        string code,
        string name,
        int sortOrder,
        int probabilityDefault,
        bool isWon,
        bool isLost,
        Guid? creatorId,
        DateTimeOffset now)
    {
        Id = id;
        EntityType = Check.NotNullOrWhiteSpace(entityType, nameof(entityType));
        Code = Check.NotNullOrWhiteSpace(code, nameof(code));
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        SortOrder = sortOrder;
        ProbabilityDefault = Math.Clamp(probabilityDefault, 0, 100);
        IsWon = isWon;
        IsLost = isLost;
        IsActive = true;
        SetCreationAudit(tenantId, creatorId, now);
    }

    public string EntityType { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public int SortOrder { get; private set; }
    public int ProbabilityDefault { get; private set; }
    public bool IsWon { get; private set; }
    public bool IsLost { get; private set; }
    public bool IsActive { get; private set; }
}

public interface IPipelineStageRepository : IRepository<PipelineStage, Guid>
{
    Task<IReadOnlyList<PipelineStage>> GetByTenantAsync(Guid tenantId, string entityType, CancellationToken cancellationToken = default);
}
