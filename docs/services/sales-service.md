# Sales Service

## Trách nhiệm

- Đơn bán hàng.
- Chính sách giá, chiết khấu.
- Giao hàng và trạng thái hoàn tất đơn.

## Data hiện tại

- SalesOrders
- SalesOrderLines
- Source trace fields
- Delivery and inventory reservation status fields

## Data dự kiến

- PricePolicies
- Discounts
- Deliveries

## API hiện tại

- `GET /api/sales/orders?tenantId={tenantId}&search={search}`
- `GET /api/sales/orders/{id}?tenantId={tenantId}`
- `POST /api/sales/orders`
- `POST /api/sales/orders/{id}/approve`
- `POST /api/sales/orders/{id}/deliver`
- `POST /api/sales/orders/{id}/complete`

## Migration

- SQL migrations:
  - `services/sales-service/migrations/001_create_sales_core.sql`
  - `services/sales-service/migrations/002_add_sales_order_source_trace.sql`
  - `services/sales-service/migrations/003_add_sales_order_delivery_status.sql`
  - `services/sales-service/migrations/004_add_sales_order_inventory_reservation.sql`
  - `services/sales-service/migrations/005_add_sales_order_pricing_fields.sql`
  - `services/sales-service/migrations/006_add_sales_order_line_warehouse.sql`
- Database: `sales_db`
- Local port: `http://localhost:7209`
- Runtime persistence: PostgreSQL via EF Core `SalesDbContext`

## Tenant Web

- Menu Sales chỉ hiển thị khi Tenant Service bật module `SALES`.
- Tenant admin có thể phân quyền theo catalog `Nexus.Sales.*` qua Permission page.
- Sales Orders form có thể được prefill từ CRM Quotation/Contract detail qua query string.

## CRM Integration

- web-tenant hỗ trợ luồng Quotation/Contract -> Sales Order bằng cách prefill khách hàng và dòng hàng trên form Sales Orders.
- Sales Order backend lưu `SourceType`, `SourceId`, `SourceNo` để trace chính thức về chứng từ CRM nguồn.
- Sales Orders list hiển thị nguồn và link ngược về Quotation hoặc Contract.
- Sales Orders search dùng `lowercase + trim + contains` trên số đơn, nguồn, trạng thái, mã hàng và mô tả dòng hàng.
- Sales Order detail hiển thị dòng hàng, chứng từ nguồn, trạng thái giữ hàng và trạng thái giao hàng.
- Approve Sales Order gọi Inventory reservation; Deliver/Complete gọi Inventory shipment.
- Sales reservation truyền warehouse theo từng dòng hàng; nếu request cũ không có kho thì fallback về `MAIN`.
- Sales Order line đã có pricing fields: subtotal, discount percent/amount, tax percent/amount và line total.
- Sales Orders form có thể chọn nhanh product từ Inventory catalog để lấy mã hàng, tên hàng, giá và thuế.
- Sales Orders form hỗ trợ thêm nhiều dòng hàng trước khi tạo đơn, mỗi dòng có warehouse riêng.
- Bước tiếp theo: invoice từ Sales Order và receivable/payment ở Accounting.

## Events

- `SalesOrderCreated`
- `SalesOrderApproved`
- `SalesOrderCompleted`
