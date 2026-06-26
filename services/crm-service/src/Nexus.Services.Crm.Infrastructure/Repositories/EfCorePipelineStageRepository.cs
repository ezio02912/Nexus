using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.Repositories;
using Nexus.Services.Crm.Domain.PipelineStages;

namespace Nexus.Services.Crm.Infrastructure.Repositories;

public sealed class EfCorePipelineStageRepository : EfCoreRepository<PipelineStage, Guid>, IPipelineStageRepository
{
    public EfCorePipelineStageRepository(NexusDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<PipelineStage>> GetByTenantAsync(
        Guid tenantId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        var normalized = entityType.Trim();
        return await Set
            .Where(x => x.TenantId == tenantId && x.EntityType == normalized)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
