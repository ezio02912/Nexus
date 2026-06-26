using System.Collections.Concurrent;
using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Repositories;

namespace Nexus.BuildingBlocks.Repositories;

public class InMemoryRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : NexusEntity<TKey>
    where TKey : notnull
{
    protected readonly ConcurrentDictionary<TKey, TEntity> Store = new();

    public Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default)
    {
        Store.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public async Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        return entity ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id '{id}' was not found.");
    }

    public Task<IReadOnlyList<TEntity>> GetListAsync(int skipCount = 0, int maxResultCount = 50, string? sorting = null, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TEntity> items = Store.Values.Skip(skipCount).Take(maxResultCount).ToArray();
        return Task.FromResult(items);
    }

    public Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((long)Store.Count);
    }

    public Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Store[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Store[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        Store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
