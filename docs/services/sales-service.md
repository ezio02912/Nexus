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

- `GET /api/sales/orders?tenantId={tenantId}`
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

## Events

- `SalesOrderCreated`
- `SalesOrderApproved`
- `SalesOrderCompleted`
