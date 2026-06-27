# Core API Endpoints

Các endpoint dưới đây là catalog endpoint đang dùng trong core services và tenant workspace. Các service nghiệp vụ mới dùng PostgreSQL theo database riêng; một số core service vẫn dùng EF migration trong project Infrastructure/API.

## Permission Service

- `GET /api/permissions`
- `GET /api/roles/{roleName}/permissions`
- `PUT /api/roles/{roleName}/permissions`
- `POST /api/permissions/check`

## Audit Service

- `GET /api/audit-logs`
- `POST /api/audit-logs`

## File Service

- `GET /api/files`
- `GET /api/files/{id}`
- `POST /api/files`
- `POST /api/file-links`

## Numbering Service

- `POST /api/numbering/next`
- `GET /api/numbering/sequences`

## Workflow Service

- `POST /api/workflow-definitions`
- `GET /api/workflow-definitions`
- `POST /api/workflow-instances`
- `POST /api/workflow-instances/{id}/approve`
- `POST /api/workflow-instances/{id}/reject`

## Tenant Service

- `GET /api/tenants`
- `GET /api/tenants/{id}`
- `POST /api/tenants`
- `PUT /api/tenants/{id}/settings`
- `POST /api/tenants/{id}/activate`
- `POST /api/tenants/{id}/suspend`
- `POST /api/tenants/{id}/modules/enable`
- `POST /api/tenants/{id}/modules/disable`

## Identity Service

- `GET /api/users`
- `GET /api/users/{id}`
- `POST /api/users`
- `POST /api/users/{id}/roles`
- `POST /api/auth/login`

## CRM Service

Tenant context via JWT / `x-tenant-id`. Full CRUD + workflow on all entities.

- `GET|POST /api/crm/customers`, `GET|PUT|DELETE /api/crm/customers/{id}`
- `GET|POST /api/crm/contacts`, `GET|PUT|DELETE /api/crm/contacts/{id}`
- `GET|POST /api/crm/leads`, `GET|PUT|DELETE /api/crm/leads/{id}`, `POST /api/crm/leads/{id}/convert`
- `GET|POST /api/crm/opportunities`, `GET|PUT|DELETE /api/crm/opportunities/{id}`, `PATCH /api/crm/opportunities/{id}/stage`
- `GET|POST /api/crm/quotations`, `GET|PUT|DELETE /api/crm/quotations/{id}`, `POST .../approve|reject|send`
- `GET|POST /api/crm/contracts`, `GET|PUT|DELETE /api/crm/contracts/{id}`, `POST .../sign|activate|terminate`
- `GET|POST /api/crm/activities`, `GET|PUT|DELETE /api/crm/activities/{id}`, `POST /api/crm/activities/{id}/complete`
- `GET /api/crm/dashboard`

## Sales Service

- `GET /api/sales/orders?tenantId={tenantId}&search={search}`
- `GET /api/sales/orders/{id}?tenantId={tenantId}`
- `POST /api/sales/orders`
- `POST /api/sales/orders/{id}/approve`
- `POST /api/sales/orders/{id}/deliver`
- `POST /api/sales/orders/{id}/complete`

## Inventory Service

- `GET /api/inventory/balances?tenantId={tenantId}&search={search}`
- `GET /api/inventory/products?tenantId={tenantId}&search={search}`
- `POST /api/inventory/products` tạo/cập nhật mã hàng hoá, gồm đơn vị, loại hàng, thuộc tính và biến thể.
- `GET /api/inventory/warehouses?tenantId={tenantId}&search={search}`
- `POST /api/inventory/warehouses`
- `POST /api/inventory/stock/import`
- `POST /api/inventory/reservations`
- `POST /api/inventory/shipments`

## Purchase Service

- `GET /api/purchase/suppliers?tenantId={tenantId}&search={search}`
- `POST /api/purchase/suppliers`
- `GET /api/purchase/orders?tenantId={tenantId}&search={search}`
- `POST /api/purchase/orders`
- `POST /api/purchase/orders/{id}/approve`
- `POST /api/purchase/orders/{id}/receive`
- `GET /api/purchase/goods-receipts?tenantId={tenantId}`
