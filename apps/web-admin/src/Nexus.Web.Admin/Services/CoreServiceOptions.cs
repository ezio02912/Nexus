namespace Nexus.Web.Admin.Services;

/// <summary>
/// Downstream service base addresses. By default every service is reached through the API
/// gateway (port 7200) using its route prefix, so the admin only needs the gateway running.
/// </summary>
public sealed class CoreServiceOptions
{
    public string Tenant { get; set; } = "http://localhost:7200/tenant";
    public string Identity { get; set; } = "http://localhost:7200/identity";
    public string Permission { get; set; } = "http://localhost:7200/permission";
    public string Audit { get; set; } = "http://localhost:7200/audit";
    public string File { get; set; } = "http://localhost:7200/file";
    public string Numbering { get; set; } = "http://localhost:7200/numbering";
    public string Workflow { get; set; } = "http://localhost:7200/workflow";
    public string MasterData { get; set; } = "http://localhost:7200/masterdata";
}
