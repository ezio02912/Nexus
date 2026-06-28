namespace Nexus.Web.Tenant.Services;

// Vietnamese labels for string-based statuses returned by the sales/inventory services.
public static class SalesLabels
{
    public static string OrderStatus(string? value) => value switch
    {
        "Draft" => "Nháp",
        "Approved" => "Đã duyệt",
        "Delivered" => "Đã giao",
        "Completed" => "Hoàn tất",
        "Cancelled" => "Đã huỷ",
        _ => value ?? "—"
    };

    public static string OrderStatusCss(string? value) => value switch
    {
        "Approved" or "Completed" => "app-status-badge app-status-active",
        "Cancelled" => "app-status-badge app-status-danger",
        _ => "app-status-badge app-status-muted"
    };

    public static string DeliveryStatus(string? value) => value switch
    {
        "Delivered" => "Đã giao",
        "Ready" => "Sẵn sàng giao",
        _ => "Chờ giao"
    };

    public static string DeliveryStatusCss(string? value) => value switch
    {
        "Delivered" => "app-status-badge app-status-active",
        "Ready" => "app-status-badge app-status-pending",
        _ => "app-status-badge app-status-muted"
    };

    public static string ReservationStatus(string? value) => value switch
    {
        "Reserved" => "Đã giữ",
        _ => "Chờ giữ"
    };

    public static string ReservationStatusCss(string? value) => value switch
    {
        "Reserved" => "app-status-badge app-status-active",
        _ => "app-status-badge app-status-muted"
    };
}

public static class InventoryLabels
{
    public static string TransferStatus(string? value) => value switch
    {
        "Completed" => "Hoàn tất",
        "Pending" => "Chờ xử lý",
        "Cancelled" => "Đã huỷ",
        _ => value ?? "—"
    };

    public static string MovementType(string? value) => value switch
    {
        "In" => "Nhập",
        "Out" => "Xuất",
        "Adjust" => "Điều chỉnh",
        "Transfer" => "Chuyển kho",
        _ => value ?? "—"
    };
}
