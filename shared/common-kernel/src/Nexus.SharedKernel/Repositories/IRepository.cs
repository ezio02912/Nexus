using Nexus.SharedKernel.Domain;

namespace Nexus.SharedKernel.Repositories;

public interface IRepository<TEntity, TKey>
    where TEntity : NexusEntity<TKey>
{
    Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default);
    Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetListAsync(int skipCount = 0, int maxResultCount = 50, string? sorting = null, CancellationToken cancellationToken = default);
    Task<long> GetCountAsync(CancellationToken cancellationToken = default);
    Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
}
