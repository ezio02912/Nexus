namespace Nexus.Web.Tenant.Services;

/// <summary>
/// Central catalog of business document attachment categories and their Vietnamese labels.
/// Each category groups files of the same business purpose on a single entity, so we can
/// require e.g. both a delivery note and a sales invoice on a sales order.
/// </summary>
public static class DocumentFileCatalog
{
    public const string Quotation = "QUOTATION";
    public const string Contract = "CONTRACT";
    public const string SalesDelivery = "SALES_DELIVERY";
    public const string SalesInvoice = "SALES_INVOICE";
    public const string PurchaseReceipt = "PURCHASE_RECEIPT";
    public const string PurchaseInvoice = "PURCHASE_INVOICE";
    public const string StockTransfer = "STOCK_TRANSFER";
    public const string ProductImage = "PRODUCT_IMAGE";

    private static readonly IReadOnlyDictionary<string, string> Labels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [Quotation] = "Tệp báo giá",
        [Contract] = "Tệp hợp đồng",
        [SalesDelivery] = "Phiếu xuất kho",
        [SalesInvoice] = "Hoá đơn bán",
        [PurchaseReceipt] = "Phiếu nhập kho",
        [PurchaseInvoice] = "Hoá đơn mua",
        [StockTransfer] = "Phiếu chuyển kho",
        [ProductImage] = "Ảnh hàng hoá",
    };

    public static string Label(string? category) =>
        category is not null && Labels.TryGetValue(category, out var label) ? label : "Tài liệu đính kèm";
}
