# Sales Service

## Trách nhiệm

- Đơn bán hàng.
- Chính sách giá, chiết khấu.
- Giao hàng và trạng thái hoàn tất đơn.

## Data dự kiến

- SalesOrders
- SalesOrderLines
- PricePolicies
- Discounts
- Deliveries

## API hiện tại

- `GET /api/sales/orders?tenantId={tenantId}&search={search}`
- `POST /api/sales/orders`
- `POST /api/sales/orders/{id}/approve`
- `POST /api/sales/orders/{id}/complete`

## Migration

- SQL migration: `services/sales-service/migrations/001_create_sales_core.sql`
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
- Bước tiếp theo: thêm Sales Order detail, nhiều dòng hàng, pricing/discount/tax và delivery status.

## Events

- `SalesOrderCreated`
- `SalesOrderApproved`
- `SalesOrderCompleted`
