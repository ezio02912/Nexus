# Purchase Service

## Trách nhiệm

- Nhà cung cấp.
- Yêu cầu mua hàng, đơn mua hàng.
- Nhận hàng, hóa đơn đầu vào, công nợ nhà cung cấp.

## Data hiện tại

- Suppliers
- PurchaseOrders
- GoodsReceipts

## Data dự kiến

- PurchaseRequests
- SupplierInvoices

## API hiện tại

- `GET /api/purchase/suppliers?tenantId={tenantId}&search={search}`
- `POST /api/purchase/suppliers`
- `GET /api/purchase/orders?tenantId={tenantId}&search={search}`
- `POST /api/purchase/orders`
- `POST /api/purchase/orders/{id}/approve`
- `POST /api/purchase/orders/{id}/receive`
- `GET /api/purchase/goods-receipts?tenantId={tenantId}`

## Migration

- SQL migration: `services/purchase-service/migrations/001_create_purchase_core.sql`
- Database: `purchase_db`
- Local port: `http://localhost:7212`
- Runtime persistence: PostgreSQL via EF Core `PurchaseDbContext`

## Inventory Integration

- Receive Purchase Order tạo Goods Receipt và gọi Inventory `POST /api/inventory/stock/import`.
- Inventory movement dùng source `PURCHASE_RECEIPT`, source id là Goods Receipt id và source no là receipt no.
- Dòng PO truyền warehouse/product/quantity sang Inventory để tăng tồn đúng kho.

## Tenant Web

- `/purchase` là page riêng cho đơn mua.
- `/purchase/suppliers` là page riêng cho nhà cung cấp.
- `/purchase/receipts` là page riêng cho phiếu nhận hàng, không dùng query tab.
- Tenant admin có thể phân quyền theo `Nexus.Purchase.*`.
- Nhận hàng từ PO sẽ cập nhật tồn kho thông qua Inventory service.

## Events

- `PurchaseOrderApproved`
- `GoodsReceived`
- `SupplierInvoiceReceived`
