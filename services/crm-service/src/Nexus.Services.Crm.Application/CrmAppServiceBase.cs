using Nexus.BuildingBlocks.Application;
using Nexus.Services.Crm.Domain;
using Nexus.SharedKernel.Context;
using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Exceptions;
using Nexus.SharedKernel.MultiTenancy;

namespace Nexus.Services.Crm.Application;

public abstract class CrmAppServiceBase : NexusAppServiceBase
{
    protected const string ServiceName = "crm-service";

    protected CrmAppServiceBase(
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        ICorrelationContext correlationContext)
        : base(currentTenant, currentUser, correlationContext)
    {
    }

    protected Guid GetRequiredTenantId()
    {
        if (!CurrentTenant.IsAvailable || !CurrentTenant.Id.HasValue)
        {
            throw new NexusBusinessException(CrmErrorCodes.InvalidStatusTransition, "Tenant context is required.");
        }

        return CurrentTenant.Id.Value;
    }

    protected void EnsureTenantAccess(IMultiTenant entity)
    {
        if (CurrentTenant.IsAvailable
            && CurrentTenant.Id.HasValue
            && entity.TenantId.HasValue
            && entity.TenantId.Value != CurrentTenant.Id.Value)
        {
            throw new NexusBusinessException(CrmErrorCodes.InvalidStatusTransition, "Access denied for this tenant.");
        }
    }

    protected void EnsureTenantAccess(Guid? entityTenantId)
    {
        if (CurrentTenant.IsAvailable
            && CurrentTenant.Id.HasValue
            && entityTenantId.HasValue
            && entityTenantId.Value != CurrentTenant.Id.Value)
        {
            throw new NexusBusinessException(CrmErrorCodes.InvalidStatusTransition, "Access denied for this tenant.");
        }
    }
}
