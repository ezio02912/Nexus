namespace Nexus.SharedKernel.Domain;

public interface IHasConcurrencyStamp
{
    string ConcurrencyStamp { get; }
}
