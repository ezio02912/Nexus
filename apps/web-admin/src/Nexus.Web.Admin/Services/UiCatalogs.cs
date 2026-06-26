using BootstrapBlazor.Components;

namespace Nexus.Web.Admin.Services;

/// <summary>
/// Shared fixed option sets used by the admin pages so dropdowns (BootstrapBlazor Select)
/// stay consistent across screens.
/// </summary>
public static class UiCatalogs
{
    public static readonly List<SelectedItem> Modules =
    [
        new("CRM", "CRM - Quan hệ khách hàng"),
        new("HRM", "HRM - Nhân sự"),
        new("ERP", "ERP - Quản trị nguồn lực"),
        new("SALES", "Sales - Bán hàng"),
        new("INVENTORY", "Inventory - Kho"),
        new("ACCOUNTING", "Accounting - Kế toán"),
        new("PURCHASE", "Purchase - Mua hàng"),
        new("INVOICE", "Invoice - Hóa đơn"),
        new("PAYROLL", "Payroll - Lương"),
        new("ATTENDANCE", "Attendance - Chấm công"),
        new("REPORT", "Report - Báo cáo"),
        new("WORKFLOW", "Workflow - Quy trình")
    ];

    public static readonly List<SelectedItem> SubscriptionPlans =
    [
        new("FREE", "Miễn phí"),
        new("STANDARD", "Thông dụng"),
        new("PREMIUM", "Nâng cao")
    ];

    public static readonly List<SelectedItem> Roles =
    [
        new("ADMIN", "Quản trị viên"),
        new("MANAGER", "Quản lý"),
        new("USER", "Người dùng"),
        new("VIEWER", "Chỉ xem")
    ];
}
