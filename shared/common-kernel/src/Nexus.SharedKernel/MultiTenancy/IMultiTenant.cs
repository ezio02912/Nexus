namespace Nexus.SharedKernel.MultiTenancy;

public interface IMultiTenant
{
    Guid? TenantId { get; }
}
