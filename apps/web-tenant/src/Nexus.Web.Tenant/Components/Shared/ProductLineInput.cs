namespace Nexus.Web.Tenant.Components.Shared;

// UI model for a single editable product line shared by quotation/contract forms.
public sealed class ProductLineInput
{
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string Unit { get; set; } = "Cái";
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;
}
