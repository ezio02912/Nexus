namespace Nexus.SharedKernel.Context;

public interface ICurrentTenant
{
    Guid? Id { get; }
    string? Name { get; }
    bool IsAvailable { get; }
}
