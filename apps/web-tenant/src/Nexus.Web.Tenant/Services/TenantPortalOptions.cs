namespace Nexus.Web.Tenant.Services;

public sealed class TenantPortalOptions
{
    public string Tenant { get; set; } = "http://localhost:7201";
    public string Identity { get; set; } = "http://localhost:7202";
    public string Permission { get; set; } = "http://localhost:7203";
    public string Crm { get; set; } = "http://localhost:7208";
    public string Sales { get; set; } = "http://localhost:7209";
    public string Inventory { get; set; } = "http://localhost:7210";
    public string MasterData { get; set; } = "http://localhost:7211";
    public string Purchase { get; set; } = "http://localhost:7212";
    public string Hrm { get; set; } = "http://localhost:7214";
    public string Attendance { get; set; } = "http://localhost:7215";
    public string Payroll { get; set; } = "http://localhost:7216";
    public string File { get; set; } = "http://localhost:7205";
}
