using Nexus.SharedKernel.UnitOfWork;

namespace Nexus.BuildingBlocks.EntityFrameworkCore.UnitOfWork;

public sealed class EfCoreUnitOfWork : IUnitOfWork
{
    private readonly NexusDbContext _context;

    public EfCoreUnitOfWork(NexusDbContext context)
    {
        _context = context;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
