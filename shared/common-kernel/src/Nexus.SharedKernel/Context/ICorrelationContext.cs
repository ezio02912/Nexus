namespace Nexus.SharedKernel.Context;

public interface ICorrelationContext
{
    string? CorrelationId { get; }
}
