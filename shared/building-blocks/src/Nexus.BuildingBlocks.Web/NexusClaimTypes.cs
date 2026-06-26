namespace Nexus.BuildingBlocks.Web;

/// <summary>
/// Custom JWT claim types shared across all Nexus services.
/// </summary>
public static class NexusClaimTypes
{
    public const string UserId = "sub";
    public const string UserName = "name";
    public const string TenantId = "tenant_id";
    public const string Permission = "permission";
    public const string Role = "role";
}
