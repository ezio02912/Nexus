using BootstrapBlazor.Components;
using Nexus.ApiContracts.Permissions;

namespace Nexus.Web.Tenant.Services;

/// <summary>
/// Tenant workspace module menu catalog (parent/child) with view permissions.
/// </summary>
public static class TenantModuleMenus
{
    public sealed record TenantMenuItem(string Text, string Url, string Icon, string ViewPermission);

    public sealed record ModuleMenuDefinition(
        string Code,
        string Title,
        string Icon,
        IReadOnlyList<TenantMenuItem> Items);

    public static IReadOnlyList<ModuleMenuDefinition> All { get; } =
    [
        new("CRM", "CRM", "fa-solid fa-address-book",
        [
            Item("Dashboard", "crm/dashboard", "fa-solid fa-gauge-high", NexusPermissions.Crm.Dashboard.View),
            Item("Khách hàng", "crm/customers", "fa-solid fa-building-user", NexusPermissions.Crm.Customers.View),
            Item("Liên hệ", "crm/contacts", "fa-solid fa-address-book", NexusPermissions.Crm.Contacts.View),
            Item("Leads", "crm/leads", "fa-solid fa-user-plus", NexusPermissions.Crm.Leads.View),
            Item("Cơ hội", "crm/opportunities", "fa-solid fa-bullseye", NexusPermissions.Crm.Opportunities.View),
            Item("Kanban", "crm/opportunity-board", "fa-solid fa-table-columns", NexusPermissions.Crm.OpportunityBoard.View),
            Item("Báo giá", "crm/quotations", "fa-solid fa-file-invoice", NexusPermissions.Crm.Quotations.View),
            Item("Hợp đồng", "crm/contracts", "fa-solid fa-file-signature", NexusPermissions.Crm.Contracts.View),
            Item("Hoạt động", "crm/activities", "fa-solid fa-calendar-check", NexusPermissions.Crm.Activities.View),
        ]),
        new("SALES", "Bán hàng", "fa-solid fa-cart-shopping",
        [
            Item("Đơn hàng", "sales/orders", "fa-solid fa-receipt", NexusPermissions.Sales.Orders.View),
        ]),
        new("PURCHASE", "Mua hàng", "fa-solid fa-cart-flatbed",
        [
            Item("Tổng quan phân hệ", "purchase", "fa-solid fa-compass", ""),
        ]),
        new("INVENTORY", "Kho", "fa-solid fa-boxes-stacked",
        [
            Item("Tổng quan phân hệ", "inventory", "fa-solid fa-compass", ""),
        ]),
        new("ERP", "ERP", "fa-solid fa-industry",
        [
            Item("Tổng quan phân hệ", "erp", "fa-solid fa-compass", ""),
        ]),
        new("INVOICE", "Hóa đơn", "fa-solid fa-file-invoice-dollar",
        [
            Item("Tổng quan phân hệ", "invoice", "fa-solid fa-compass", ""),
        ]),
        new("ACCOUNTING", "Kế toán", "fa-solid fa-calculator",
        [
            Item("Tổng quan phân hệ", "accounting", "fa-solid fa-compass", ""),
        ]),
        new("PAYROLL", "Lương", "fa-solid fa-money-check-dollar",
        [
            Item("Tổng quan phân hệ", "payroll", "fa-solid fa-compass", ""),
        ]),
        new("HRM", "Nhân sự", "fa-solid fa-people-group",
        [
            Item("Tổng quan phân hệ", "hrm", "fa-solid fa-compass", ""),
        ]),
        new("ATTENDANCE", "Chấm công", "fa-solid fa-clock",
        [
            Item("Tổng quan phân hệ", "attendance", "fa-solid fa-compass", ""),
        ]),
        new("WORKFLOW", "Quy trình", "fa-solid fa-diagram-project",
        [
            Item("Quy trình phê duyệt", "workflow", "fa-solid fa-list-check", NexusPermissions.Workflow.View),
        ]),
        new("REPORT", "Báo cáo", "fa-solid fa-chart-line",
        [
            Item("Báo cáo tổng hợp", "reports", "fa-solid fa-chart-pie", NexusPermissions.Workflow.View),
        ]),
    ];

    public static IEnumerable<ModuleMenuDefinition> GetEnabledModules(Func<string, bool> hasModule, Func<string, bool> isGranted)
    {
        foreach (var module in All.Where(x => hasModule(x.Code)))
        {
            var items = module.Items
                .Where(x => string.IsNullOrEmpty(x.ViewPermission) || isGranted(x.ViewPermission))
                .ToList();
            if (items.Count == 0)
            {
                continue;
            }

            yield return module with { Items = items };
        }
    }

    public static IEnumerable<ModuleMenuDefinition> GetEnabledModules(Func<string, bool> hasModule) =>
        GetEnabledModules(hasModule, _ => true);

    public static IEnumerable<MenuItem> ToMenuItems(IEnumerable<TenantMenuItem> items) =>
        items.Select(x => new MenuItem(x.Text, x.Url, x.Icon));

    private static TenantMenuItem Item(string text, string url, string icon, string viewPermission) =>
        new(text, url, icon, viewPermission);
}

public static class TenantAdminMenus
{
    public sealed record AdminMenuItem(string Text, string Url, string Icon, string ViewPermission);

    public static IReadOnlyList<AdminMenuItem> All { get; } =
    [
        new("Người dùng", "tenant-users", "fa-solid fa-users", NexusPermissions.TenantAdmin.Users.View),
        new("Phân quyền", "tenant-permissions", "fa-solid fa-user-shield", NexusPermissions.TenantAdmin.Permissions.View),
        new("Thiết lập", "settings", "fa-solid fa-gear", NexusPermissions.TenantAdmin.Settings.View),
    ];

    public static IEnumerable<AdminMenuItem> GetVisibleMenus(Func<string, bool> isGranted) =>
        All.Where(x => isGranted(x.ViewPermission));

    public static IEnumerable<MenuItem> ToMenuItems(IEnumerable<AdminMenuItem> items) =>
        items.Select(x => new MenuItem(x.Text, x.Url, x.Icon));
}
