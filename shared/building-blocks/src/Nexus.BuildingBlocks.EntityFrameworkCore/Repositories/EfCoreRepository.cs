using Microsoft.EntityFrameworkCore;
using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Repositories;

namespace Nexus.BuildingBlocks.EntityFrameworkCore.Repositories;

/// <summary>
/// Generic EF Core repository. Each write operation persists immediately so existing
/// application services keep working without an explicit unit-of-work commit. Soft delete
/// is applied automatically for full-audited aggregates.
/// </summary>
public class EfCoreRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : NexusEntity<TKey>
    where TKey : notnull
{
    protected NexusDbContext Context { get; }

    protected DbSet<TEntity> Set => Context.Set<TEntity>();

    public EfCoreRepository(NexusDbContext context)
    {
        Context = context;
    }

    public virtual async Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await Set.FindAsync([id], cancellationToken);
    }

    public virtual async Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        return entity ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id '{id}' was not found.");
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetListAsync(int skipCount = 0, int maxResultCount = 50, string? sorting = null, CancellationToken cancellationToken = default)
    {
        return await Set
            .OrderBy(x => x.Id)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await Set.LongCountAsync(cancellationToken);
    }

    public virtual async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Set.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Set.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        if (entity is ISoftDelete softDelete)
        {
            softDelete.MarkDeleted(null, DateTimeOffset.UtcNow);
            Set.Update(entity);
        }
        else
        {
            Set.Remove(entity);
        }

        await Context.SaveChangesAsync(cancellationToken);
    }
}
