using Nexus.SharedKernel.Context;

namespace Nexus.BuildingBlocks.Application;

public abstract class NexusAppServiceBase : IApplicationService
{
    protected NexusAppServiceBase(ICurrentTenant currentTenant, ICurrentUser currentUser, ICorrelationContext correlationContext)
    {
        CurrentTenant = currentTenant;
        CurrentUser = currentUser;
        CorrelationContext = correlationContext;
    }

    protected ICurrentTenant CurrentTenant { get; }
    protected ICurrentUser CurrentUser { get; }
    protected ICorrelationContext CorrelationContext { get; }
}
