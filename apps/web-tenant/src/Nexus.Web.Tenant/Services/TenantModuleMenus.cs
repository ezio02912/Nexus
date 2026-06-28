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
            Item("Leads", "crm/leads", "fa-solid fa-user-plus", NexusPermissions.Crm.Leads.View),
            Item("Khách hàng", "crm/customers", "fa-solid fa-building-user", NexusPermissions.Crm.Customers.View),
            Item("Liên hệ", "crm/contacts", "fa-solid fa-address-book", NexusPermissions.Crm.Contacts.View),
            Item("Cơ hội", "crm/opportunities", "fa-solid fa-bullseye", NexusPermissions.Crm.Opportunities.View),
            Item("Báo giá", "crm/quotations", "fa-solid fa-file-invoice", NexusPermissions.Crm.Quotations.View),
            Item("Hợp đồng", "crm/contracts", "fa-solid fa-file-signature", NexusPermissions.Crm.Contracts.View),
            Item("Hoạt động", "crm/activities", "fa-solid fa-calendar-check", NexusPermissions.Crm.Activities.View),
        ]),
        new("PURCHASE", "Hàng hoá", "fa-solid fa-cart-flatbed", Groups.Supply,
        [
            Item("Đơn bán hàng", "sales/orders", "fa-solid fa-receipt", NexusPermissions.Sales.Orders.View),
            Item("Đơn mua hàng", "purchase", "fa-solid fa-cart-flatbed", NexusPermissions.Purchase.Orders.View),
            Item("Phiếu nhận hàng", "purchase/receipts", "fa-solid fa-truck-ramp-box", NexusPermissions.Purchase.Orders.Receive),
            Item("Tồn kho", "inventory", "fa-solid fa-boxes-stacked", NexusPermissions.Inventory.Stock.View),
            Item("Chuyển kho", "inventory/transfers", "fa-solid fa-right-left", NexusPermissions.Inventory.Stock.Transfer),
            Item("Danh mục nhà cung cấp", "purchase/suppliers", "fa-solid fa-truck-field", NexusPermissions.Purchase.Suppliers.View),
            Item("Danh mục hàng hoá", "inventory/products", "fa-solid fa-box", NexusPermissions.Inventory.Products.View),
            Item("Danh mục kho hàng", "inventory/warehouses", "fa-solid fa-warehouse", NexusPermissions.Inventory.Warehouses.View),
        ]),
        new("INVOICE", "Hóa đơn", "fa-solid fa-file-invoice-dollar", Groups.Finance,
        [
            Item("Tổng quan phân hệ", "invoice", "fa-solid fa-compass", ""),
        ]),
        new("ACCOUNTING", "Kế toán", "fa-solid fa-calculator", Groups.Finance,
        [
            Item("Tổng quan phân hệ", "accounting", "fa-solid fa-compass", ""),
        ]),
        new("HRM", "Nhân sự", "fa-solid fa-people-group", Groups.Hr,
        [
            Item("Dashboard", "hrm", "fa-solid fa-gauge-high", NexusPermissions.Hrm.Dashboard.View),
            Item("Nhân viên", "hrm/employees", "fa-solid fa-id-card", NexusPermissions.Hrm.Employees.View),
            Item("Phòng ban", "hrm/departments", "fa-solid fa-sitemap", NexusPermissions.Hrm.Departments.View),
            Item("Chức vụ", "hrm/positions", "fa-solid fa-user-tie", NexusPermissions.Hrm.Positions.View),
            Item("Hợp đồng", "hrm/contracts", "fa-solid fa-file-signature", NexusPermissions.Hrm.Contracts.View),
            Item("Hồ sơ", "hrm/profiles", "fa-solid fa-folder-open", NexusPermissions.Hrm.Employees.View),
            Item("Tuyển dụng", "hrm/recruitment", "fa-solid fa-user-plus", NexusPermissions.Hrm.Recruitment.View),
            Item("Onboarding/Offboarding", "hrm/onboarding", "fa-solid fa-list-check", NexusPermissions.Hrm.Onboarding.View),
        ]),
        new("ATTENDANCE", "Chấm công", "fa-solid fa-clock", Groups.Hr,
        [
            Item("Lịch làm việc", "attendance/work-calendars", "fa-solid fa-calendar-days", NexusPermissions.Attendance.WorkCalendars.View),
            Item("Ca làm", "attendance/shifts", "fa-solid fa-business-time", NexusPermissions.Attendance.Shifts.View),
            Item("Phân ca", "attendance/shift-assignments", "fa-solid fa-calendar-plus", NexusPermissions.Attendance.Shifts.View),
            Item("Chấm công", "attendance/records", "fa-solid fa-clock-rotate-left", NexusPermissions.Attendance.Records.View),
            Item("Đơn phép", "attendance/leaves", "fa-solid fa-person-walking-arrow-right", NexusPermissions.Attendance.LeaveRequests.View),
            Item("Tăng ca", "attendance/overtime", "fa-solid fa-stopwatch", NexusPermissions.Attendance.OvertimeRequests.View),
            Item("Bảng công", "attendance/timesheets", "fa-solid fa-table-list", NexusPermissions.Attendance.Timesheets.View),
        ]),
        new("PAYROLL", "Lương", "fa-solid fa-money-check-dollar", Groups.Hr,
        [
            Item("Cấu hình lương", "payroll/settings", "fa-solid fa-sliders", NexusPermissions.Payroll.Settings.View),
            Item("Thành phần lương", "payroll/components", "fa-solid fa-layer-group", NexusPermissions.Payroll.Components.View),
            Item("Kỳ lương", "payroll/periods", "fa-solid fa-calendar-check", NexusPermissions.Payroll.Periods.View),
            Item("Bảng lương", "payroll/runs", "fa-solid fa-table", NexusPermissions.Payroll.Runs.View),
            Item("Phiếu lương", "payroll/payslips", "fa-solid fa-receipt", NexusPermissions.Payroll.Payslips.View),
            Item("Chi lương", "payroll/payments", "fa-solid fa-building-columns", NexusPermissions.Payroll.Payments.View),
            Item("Báo cáo lương", "payroll/reports", "fa-solid fa-chart-column", NexusPermissions.Payroll.Runs.View),
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
