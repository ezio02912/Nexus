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
        string Group,
        IReadOnlyList<TenantMenuItem> Items);

    public sealed record ModuleGroupDefinition(
        string Code,
        string Title,
        string Icon,
        IReadOnlyList<ModuleMenuDefinition> Modules);

    // Functional domains used to group business modules in the tenant sidebar.
    public static class Groups
    {
        public const string Sales = "GROUP_SALES";
        public const string Supply = "GROUP_SUPPLY";
        public const string Finance = "GROUP_FINANCE";
        public const string Hr = "GROUP_HR";
        public const string System = "GROUP_SYSTEM";
    }

    private static readonly IReadOnlyList<(string Code, string Title, string Icon)> GroupOrder =
    [
        (Groups.Sales, "Kinh doanh", "fa-solid fa-handshake"),
        (Groups.Supply, "Mua hàng & Kho", "fa-solid fa-warehouse"),
        (Groups.Finance, "Tài chính - Kế toán", "fa-solid fa-coins"),
        (Groups.Hr, "Nhân sự", "fa-solid fa-users-gear"),
        (Groups.System, "Hệ thống & Báo cáo", "fa-solid fa-gears"),
    ];

    public static IReadOnlyList<ModuleMenuDefinition> All { get; } =
    [
        new("CRM", "CRM", "fa-solid fa-address-book", Groups.Sales,
        [
            Item("Dashboard", "crm/dashboard", "fa-solid fa-gauge-high", NexusPermissions.Crm.Dashboard.View),
            Item("Khách hàng", "crm/customers", "fa-solid fa-building-user", NexusPermissions.Crm.Customers.View),
            Item("Liên hệ", "crm/contacts", "fa-solid fa-address-book", NexusPermissions.Crm.Contacts.View),
            Item("Leads", "crm/leads", "fa-solid fa-user-plus", NexusPermissions.Crm.Leads.View),
            Item("Cơ hội", "crm/opportunities", "fa-solid fa-bullseye", NexusPermissions.Crm.Opportunities.View),
            Item("Báo giá", "crm/quotations", "fa-solid fa-file-invoice", NexusPermissions.Crm.Quotations.View),
            Item("Hợp đồng", "crm/contracts", "fa-solid fa-file-signature", NexusPermissions.Crm.Contracts.View),
            Item("Hoạt động", "crm/activities", "fa-solid fa-calendar-check", NexusPermissions.Crm.Activities.View),
            Item("Đơn hàng", "sales/orders", "fa-solid fa-receipt", NexusPermissions.Sales.Orders.View),
        ]),
        new("PURCHASE", "Mua hàng", "fa-solid fa-cart-flatbed", Groups.Supply,
        [
            Item("Đơn mua", "purchase", "fa-solid fa-cart-flatbed", NexusPermissions.Purchase.Orders.View),
            Item("Nhà cung cấp", "purchase/suppliers", "fa-solid fa-truck-field", NexusPermissions.Purchase.Suppliers.View),
            Item("Phiếu nhận hàng", "purchase/receipts", "fa-solid fa-truck-ramp-box", NexusPermissions.Purchase.Orders.Receive),
            Item("Tồn kho", "inventory", "fa-solid fa-boxes-stacked", NexusPermissions.Inventory.Stock.View),
            Item("Mã hàng hoá", "inventory/products", "fa-solid fa-box", NexusPermissions.Inventory.Products.View),
            Item("Kho hàng", "inventory/warehouses", "fa-solid fa-warehouse", NexusPermissions.Inventory.Warehouses.View),
        ]),
        new("INVOICE", "Hóa đơn", "fa-solid fa-file-invoice-dollar", Groups.Finance,
        [
            Item("Mua hàng", "purchase", "fa-solid fa-cart-flatbed", NexusPermissions.Purchase.Orders.View),
            Item("Tổng quan phân hệ", "invoice", "fa-solid fa-compass", ""),
        ]),
        new("ACCOUNTING", "Kế toán", "fa-solid fa-calculator", Groups.Finance,
        [
            Item("Tổng quan phân hệ", "accounting", "fa-solid fa-compass", ""),
        ]),
        new("HRM", "Nhân sự", "fa-solid fa-people-group", Groups.Hr,
        [
            Item("Tổng quan phân hệ", "hrm", "fa-solid fa-compass", ""),
        ]),
        new("ATTENDANCE", "Chấm công", "fa-solid fa-clock", Groups.Hr,
        [
            Item("Tổng quan phân hệ", "attendance", "fa-solid fa-compass", ""),
        ]),
        new("PAYROLL", "Lương", "fa-solid fa-money-check-dollar", Groups.Hr,
        [
            Item("Tổng quan phân hệ", "payroll", "fa-solid fa-compass", ""),
        ]),
        new("ERP", "ERP", "fa-solid fa-industry", Groups.System,
        [
            Item("Tổng quan phân hệ", "erp", "fa-solid fa-compass", ""),
        ]),
        new("WORKFLOW", "Quy trình", "fa-solid fa-diagram-project", Groups.System,
        [
            Item("Quy trình phê duyệt", "workflow", "fa-solid fa-list-check", NexusPermissions.Workflow.View),
        ]),
        new("REPORT", "Báo cáo", "fa-solid fa-chart-line", Groups.System,
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

    /// <summary>
    /// Returns the enabled modules grouped into functional domains, preserving the catalog order.
    /// Only groups that contain at least one visible module are returned.
    /// </summary>
    public static IEnumerable<ModuleGroupDefinition> GetEnabledGroups(Func<string, bool> hasModule, Func<string, bool> isGranted)
    {
        var enabled = GetEnabledModules(hasModule, isGranted).ToList();

        foreach (var (code, title, icon) in GroupOrder)
        {
            var modules = enabled.Where(x => x.Group == code).ToList();
            if (modules.Count == 0)
            {
                continue;
            }

            yield return new ModuleGroupDefinition(code, title, icon, modules);
        }
    }

    public static IEnumerable<ModuleGroupDefinition> GetEnabledGroups(Func<string, bool> hasModule) =>
        GetEnabledGroups(hasModule, _ => true);

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
        new("Gói dịch vụ", "subscription", "fa-solid fa-tags", NexusPermissions.TenantAdmin.Settings.View),
        new("Thiết lập", "settings", "fa-solid fa-gear", NexusPermissions.TenantAdmin.Settings.View),
    ];

    public static IEnumerable<AdminMenuItem> GetVisibleMenus(Func<string, bool> isGranted) =>
        All.Where(x => isGranted(x.ViewPermission));

    public static IEnumerable<MenuItem> ToMenuItems(IEnumerable<AdminMenuItem> items) =>
        items.Select(x => new MenuItem(x.Text, x.Url, x.Icon));
}
