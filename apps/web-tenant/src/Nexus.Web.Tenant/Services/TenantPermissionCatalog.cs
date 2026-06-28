using Nexus.ApiContracts.Permissions;

namespace Nexus.Web.Tenant.Services;

/// <summary>
/// Tenant permission catalog grouped by workspace menu areas (for the role editor UI).
/// </summary>
public static class TenantPermissionCatalog
{
    public sealed record PermissionAction(string Key, string Label);
    public sealed record MenuPermissionGroup(string MenuLabel, IReadOnlyList<PermissionAction> Actions);
    public sealed record AreaPermissionGroup(string AreaLabel, IReadOnlyList<MenuPermissionGroup> Menus);

    public static IReadOnlyList<AreaPermissionGroup> Areas { get; } =
    [
        new("CRM", [
            Menu("Dashboard", [Action(NexusPermissions.Crm.Dashboard.View, "Xem")]),
            Menu("Khách hàng", Crud(
                NexusPermissions.Crm.Customers.View,
                NexusPermissions.Crm.Customers.Create,
                NexusPermissions.Crm.Customers.Edit,
                NexusPermissions.Crm.Customers.Delete)),
            Menu("Liên hệ", Crud(
                NexusPermissions.Crm.Contacts.View,
                NexusPermissions.Crm.Contacts.Create,
                NexusPermissions.Crm.Contacts.Edit,
                NexusPermissions.Crm.Contacts.Delete)),
            Menu("Leads", Crud(
                NexusPermissions.Crm.Leads.View,
                NexusPermissions.Crm.Leads.Create,
                NexusPermissions.Crm.Leads.Edit,
                NexusPermissions.Crm.Leads.Delete)),
            Menu("Cơ hội", Crud(
                NexusPermissions.Crm.Opportunities.View,
                NexusPermissions.Crm.Opportunities.Create,
                NexusPermissions.Crm.Opportunities.Edit,
                NexusPermissions.Crm.Opportunities.Delete)),
            Menu("Kanban", [
                Action(NexusPermissions.Crm.OpportunityBoard.View, "Xem"),
                Action(NexusPermissions.Crm.OpportunityBoard.Edit, "Sửa giai đoạn")
            ]),
            Menu("Báo giá", [
                Action(NexusPermissions.Crm.Quotations.View, "Xem"),
                Action(NexusPermissions.Crm.Quotations.Create, "Thêm"),
                Action(NexusPermissions.Crm.Quotations.Edit, "Sửa"),
                Action(NexusPermissions.Crm.Quotations.Delete, "Xoá"),
                Action(NexusPermissions.Crm.Quotations.Approve, "Duyệt")
            ]),
            Menu("Hợp đồng", [
                Action(NexusPermissions.Crm.Contracts.View, "Xem"),
                Action(NexusPermissions.Crm.Contracts.Create, "Thêm"),
                Action(NexusPermissions.Crm.Contracts.Edit, "Sửa"),
                Action(NexusPermissions.Crm.Contracts.Delete, "Xoá"),
                Action(NexusPermissions.Crm.Contracts.Sign, "Ký")
            ]),
            Menu("Hoạt động", [
                Action(NexusPermissions.Crm.Activities.View, "Xem"),
                Action(NexusPermissions.Crm.Activities.Create, "Thêm"),
                Action(NexusPermissions.Crm.Activities.Edit, "Sửa"),
                Action(NexusPermissions.Crm.Activities.Delete, "Xoá"),
                Action(NexusPermissions.Crm.Activities.Complete, "Hoàn thành")
            ])
        ]),
        new("Bán hàng", [
            Menu("Đơn hàng", [
                Action(NexusPermissions.Sales.Orders.View, "Xem"),
                Action(NexusPermissions.Sales.Orders.Create, "Thêm"),
                Action(NexusPermissions.Sales.Orders.Edit, "Sửa"),
                Action(NexusPermissions.Sales.Orders.Delete, "Xoá"),
                Action(NexusPermissions.Sales.Orders.Approve, "Duyệt"),
                Action(NexusPermissions.Sales.Orders.Complete, "Hoàn thành")
            ])
        ]),
        new("Mua hàng", [
            Menu("Nhà cung cấp", [
                Action(NexusPermissions.Purchase.Suppliers.View, "Xem"),
                Action(NexusPermissions.Purchase.Suppliers.Manage, "Quản lý")
            ]),
            Menu("Đơn mua hàng", [
                Action(NexusPermissions.Purchase.Orders.View, "Xem"),
                Action(NexusPermissions.Purchase.Orders.Create, "Thêm"),
                Action(NexusPermissions.Purchase.Orders.Approve, "Duyệt"),
                Action(NexusPermissions.Purchase.Orders.Receive, "Nhận hàng")
            ])
        ]),
        new("Kho", [
            Menu("Tồn kho", [
                Action(NexusPermissions.Inventory.Stock.View, "Xem"),
                Action(NexusPermissions.Inventory.Stock.Import, "Nhập kho"),
                Action(NexusPermissions.Inventory.Stock.Reserve, "Giữ hàng"),
                Action(NexusPermissions.Inventory.Stock.Ship, "Xuất kho"),
                Action(NexusPermissions.Inventory.Stock.Transfer, "Chuyển kho")
            ]),
            Menu("Sản phẩm", [
                Action(NexusPermissions.Inventory.Products.View, "Xem"),
                Action(NexusPermissions.Inventory.Products.Manage, "Quản lý")
            ]),
            Menu("Kho hàng", [
                Action(NexusPermissions.Inventory.Warehouses.View, "Xem"),
                Action(NexusPermissions.Inventory.Warehouses.Manage, "Quản lý")
            ])
        ]),
        new("Nhân sự", [
            Menu("Dashboard", [Action(NexusPermissions.Hrm.Dashboard.View, "Xem")]),
            Menu("Nhân viên", [
                Action(NexusPermissions.Hrm.Employees.View, "Xem"),
                Action(NexusPermissions.Hrm.Employees.Create, "Thêm"),
                Action(NexusPermissions.Hrm.Employees.Edit, "Sửa"),
                Action(NexusPermissions.Hrm.Employees.Delete, "Xoá"),
                Action(NexusPermissions.Hrm.Employees.Import, "Nhập"),
                Action(NexusPermissions.Hrm.Employees.Export, "Xuất"),
                Action(NexusPermissions.Hrm.Employees.Manage, "Quản lý")
            ]),
            Menu("Phòng ban", ManageCrud(
                NexusPermissions.Hrm.Departments.View,
                NexusPermissions.Hrm.Departments.Create,
                NexusPermissions.Hrm.Departments.Edit,
                NexusPermissions.Hrm.Departments.Delete,
                NexusPermissions.Hrm.Departments.Manage)),
            Menu("Chức vụ", ManageCrud(
                NexusPermissions.Hrm.Positions.View,
                NexusPermissions.Hrm.Positions.Create,
                NexusPermissions.Hrm.Positions.Edit,
                NexusPermissions.Hrm.Positions.Delete,
                NexusPermissions.Hrm.Positions.Manage)),
            Menu("Hợp đồng", ApproveManageCrud(
                NexusPermissions.Hrm.Contracts.View,
                NexusPermissions.Hrm.Contracts.Create,
                NexusPermissions.Hrm.Contracts.Edit,
                NexusPermissions.Hrm.Contracts.Delete,
                NexusPermissions.Hrm.Contracts.Approve,
                NexusPermissions.Hrm.Contracts.Manage)),
            Menu("Tuyển dụng", ApproveManageCrud(
                NexusPermissions.Hrm.Recruitment.View,
                NexusPermissions.Hrm.Recruitment.Create,
                NexusPermissions.Hrm.Recruitment.Edit,
                NexusPermissions.Hrm.Recruitment.Delete,
                NexusPermissions.Hrm.Recruitment.Approve,
                NexusPermissions.Hrm.Recruitment.Manage)),
            Menu("Onboarding/Offboarding", ApproveManageCrud(
                NexusPermissions.Hrm.Onboarding.View,
                NexusPermissions.Hrm.Onboarding.Create,
                NexusPermissions.Hrm.Onboarding.Edit,
                NexusPermissions.Hrm.Onboarding.Delete,
                NexusPermissions.Hrm.Onboarding.Approve,
                NexusPermissions.Hrm.Onboarding.Manage))
        ]),
        new("Chấm công", [
            Menu("Lịch làm việc", ManageCrud(
                NexusPermissions.Attendance.WorkCalendars.View,
                NexusPermissions.Attendance.WorkCalendars.Create,
                NexusPermissions.Attendance.WorkCalendars.Edit,
                NexusPermissions.Attendance.WorkCalendars.Delete,
                NexusPermissions.Attendance.WorkCalendars.Manage)),
            Menu("Ca làm", ManageCrud(
                NexusPermissions.Attendance.Shifts.View,
                NexusPermissions.Attendance.Shifts.Create,
                NexusPermissions.Attendance.Shifts.Edit,
                NexusPermissions.Attendance.Shifts.Delete,
                NexusPermissions.Attendance.Shifts.Manage)),
            Menu("Chấm công", [
                Action(NexusPermissions.Attendance.Records.View, "Xem"),
                Action(NexusPermissions.Attendance.Records.Create, "Thêm"),
                Action(NexusPermissions.Attendance.Records.Edit, "Sửa"),
                Action(NexusPermissions.Attendance.Records.Delete, "Xoá"),
                Action(NexusPermissions.Attendance.Records.Approve, "Duyệt"),
                Action(NexusPermissions.Attendance.Records.Import, "Nhập"),
                Action(NexusPermissions.Attendance.Records.Export, "Xuất")
            ]),
            Menu("Đơn phép", ApproveCrud(
                NexusPermissions.Attendance.LeaveRequests.View,
                NexusPermissions.Attendance.LeaveRequests.Create,
                NexusPermissions.Attendance.LeaveRequests.Edit,
                NexusPermissions.Attendance.LeaveRequests.Delete,
                NexusPermissions.Attendance.LeaveRequests.Approve,
                NexusPermissions.Attendance.LeaveRequests.Export)),
            Menu("Tăng ca", ApproveCrud(
                NexusPermissions.Attendance.OvertimeRequests.View,
                NexusPermissions.Attendance.OvertimeRequests.Create,
                NexusPermissions.Attendance.OvertimeRequests.Edit,
                NexusPermissions.Attendance.OvertimeRequests.Delete,
                NexusPermissions.Attendance.OvertimeRequests.Approve,
                NexusPermissions.Attendance.OvertimeRequests.Export)),
            Menu("Bảng công", [
                Action(NexusPermissions.Attendance.Timesheets.View, "Xem"),
                Action(NexusPermissions.Attendance.Timesheets.Approve, "Duyệt"),
                Action(NexusPermissions.Attendance.Timesheets.Export, "Xuất"),
                Action(NexusPermissions.Attendance.Timesheets.Manage, "Quản lý")
            ])
        ]),
        new("Lương", [
            Menu("Cấu hình lương", ManageCrud(
                NexusPermissions.Payroll.Settings.View,
                NexusPermissions.Payroll.Settings.Create,
                NexusPermissions.Payroll.Settings.Edit,
                NexusPermissions.Payroll.Settings.Delete,
                NexusPermissions.Payroll.Settings.Manage)),
            Menu("Thành phần lương", ManageCrud(
                NexusPermissions.Payroll.Components.View,
                NexusPermissions.Payroll.Components.Create,
                NexusPermissions.Payroll.Components.Edit,
                NexusPermissions.Payroll.Components.Delete,
                NexusPermissions.Payroll.Components.Manage)),
            Menu("Kỳ lương", ManageCrud(
                NexusPermissions.Payroll.Periods.View,
                NexusPermissions.Payroll.Periods.Create,
                NexusPermissions.Payroll.Periods.Edit,
                NexusPermissions.Payroll.Periods.Delete,
                NexusPermissions.Payroll.Periods.Manage)),
            Menu("Bảng lương", [
                Action(NexusPermissions.Payroll.Runs.View, "Xem"),
                Action(NexusPermissions.Payroll.Runs.Create, "Thêm"),
                Action(NexusPermissions.Payroll.Runs.Edit, "Sửa"),
                Action(NexusPermissions.Payroll.Runs.Delete, "Xoá"),
                Action(NexusPermissions.Payroll.Runs.Approve, "Duyệt"),
                Action(NexusPermissions.Payroll.Runs.Export, "Xuất"),
                Action(NexusPermissions.Payroll.Runs.Manage, "Quản lý")
            ]),
            Menu("Phiếu lương", ManageCrudExport(
                NexusPermissions.Payroll.Payslips.View,
                NexusPermissions.Payroll.Payslips.Create,
                NexusPermissions.Payroll.Payslips.Edit,
                NexusPermissions.Payroll.Payslips.Delete,
                NexusPermissions.Payroll.Payslips.Export,
                NexusPermissions.Payroll.Payslips.Manage)),
            Menu("Chi lương", [
                Action(NexusPermissions.Payroll.Payments.View, "Xem"),
                Action(NexusPermissions.Payroll.Payments.Create, "Thêm"),
                Action(NexusPermissions.Payroll.Payments.Edit, "Sửa"),
                Action(NexusPermissions.Payroll.Payments.Delete, "Xoá"),
                Action(NexusPermissions.Payroll.Payments.Approve, "Duyệt"),
                Action(NexusPermissions.Payroll.Payments.Export, "Xuất"),
                Action(NexusPermissions.Payroll.Payments.Manage, "Quản lý")
            ])
        ]),
        new("Tài liệu", [
            Menu("Tệp đính kèm", [
                Action(NexusPermissions.Files.View, "Xem"),
                Action(NexusPermissions.Files.Upload, "Tải lên"),
                Action(NexusPermissions.Files.Delete, "Xoá")
            ])
        ]),
        new("Quy trình", [
            Menu("Quy trình phê duyệt", [
                Action(NexusPermissions.Workflow.View, "Xem"),
                Action(NexusPermissions.Workflow.Approve, "Phê duyệt")
            ])
        ]),
        new("Quản trị tenant", [
            Menu("Người dùng", Crud(
                NexusPermissions.TenantAdmin.Users.View,
                NexusPermissions.TenantAdmin.Users.Create,
                NexusPermissions.TenantAdmin.Users.Edit,
                NexusPermissions.TenantAdmin.Users.Delete)),
            Menu("Phân quyền", [
                Action(NexusPermissions.TenantAdmin.Permissions.View, "Xem"),
                Action(NexusPermissions.TenantAdmin.Permissions.Manage, "Quản lý")
            ]),
            Menu("Thiết lập", [
                Action(NexusPermissions.TenantAdmin.Settings.View, "Xem"),
                Action(NexusPermissions.TenantAdmin.Settings.Edit, "Sửa")
            ])
        ])
    ];

    public static IEnumerable<AreaPermissionGroup> GetVisibleGroups(IEnumerable<string> catalog, string? filter = null)
    {
        var catalogSet = new HashSet<string>(catalog, StringComparer.OrdinalIgnoreCase);
        foreach (var area in Areas)
        {
            var menus = new List<MenuPermissionGroup>();
            foreach (var menu in area.Menus)
            {
                var actions = menu.Actions
                    .Where(x => catalogSet.Contains(x.Key))
                    .Where(x => MatchesFilter(area.AreaLabel, menu.MenuLabel, x, filter))
                    .ToList();
                if (actions.Count > 0)
                {
                    menus.Add(new MenuPermissionGroup(menu.MenuLabel, actions));
                }
            }

            if (menus.Count > 0)
            {
                yield return new AreaPermissionGroup(area.AreaLabel, menus);
            }
        }
    }

    /// <summary>Extracts the service segment from a permission key (e.g. Nexus.Crm.Customers.View → Nexus.Crm.Customers).</summary>
    public static string GetServiceKey(string permission)
    {
        var parts = permission.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length >= 3 ? string.Join('.', parts[..^1]) : permission;
    }

    public static string FormatPermissionLabel(string permission)
    {
        foreach (var area in Areas)
        {
            foreach (var menu in area.Menus)
            {
                foreach (var action in menu.Actions)
                {
                    if (string.Equals(action.Key, permission, StringComparison.OrdinalIgnoreCase))
                    {
                        return $"{area.AreaLabel} / {menu.MenuLabel} — {action.Label}";
                    }
                }
            }
        }

        return permission;
    }

    public static IReadOnlyList<string> AllPermissionKeys() =>
        Areas.SelectMany(a => a.Menus).SelectMany(m => m.Actions).Select(a => a.Key).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private static bool MatchesFilter(string areaLabel, string menuLabel, PermissionAction action, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return true;
        }

        var term = filter.Trim().ToLowerInvariant();
        return action.Key.ToLowerInvariant().Contains(term)
            || action.Label.ToLowerInvariant().Contains(term)
            || areaLabel.ToLowerInvariant().Contains(term)
            || menuLabel.ToLowerInvariant().Contains(term)
            || GetServiceKey(action.Key).ToLowerInvariant().Contains(term);
    }

    private static MenuPermissionGroup Menu(string label, IReadOnlyList<PermissionAction> actions) =>
        new(label, actions);

    private static PermissionAction Action(string key, string label) => new(key, label);

    private static IReadOnlyList<PermissionAction> Crud(string view, string create, string edit, string delete) =>
    [
        Action(view, "Xem"),
        Action(create, "Thêm"),
        Action(edit, "Sửa"),
        Action(delete, "Xoá")
    ];

    private static IReadOnlyList<PermissionAction> ManageCrud(string view, string create, string edit, string delete, string manage) =>
    [
        .. Crud(view, create, edit, delete),
        Action(manage, "Quản lý")
    ];

    private static IReadOnlyList<PermissionAction> ApproveCrud(string view, string create, string edit, string delete, string approve, string export) =>
    [
        .. Crud(view, create, edit, delete),
        Action(approve, "Duyệt"),
        Action(export, "Xuất")
    ];

    private static IReadOnlyList<PermissionAction> ApproveManageCrud(string view, string create, string edit, string delete, string approve, string manage) =>
    [
        .. Crud(view, create, edit, delete),
        Action(approve, "Duyệt"),
        Action(manage, "Quản lý")
    ];

    private static IReadOnlyList<PermissionAction> ManageCrudExport(string view, string create, string edit, string delete, string export, string manage) =>
    [
        .. Crud(view, create, edit, delete),
        Action(export, "Xuất"),
        Action(manage, "Quản lý")
    ];
}
