namespace Nexus.SharedKernel.Domain;

public abstract class NexusEntity<TKey>
{
    public TKey Id { get; protected set; } = default!;
}
