using Nexus.SharedKernel.UnitOfWork;

namespace Nexus.BuildingBlocks.UnitOfWork;

public sealed class NoopUnitOfWork : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
