# Core API Endpoints

Các endpoint dưới đây là bản core in-memory để bắt đầu phát triển flow SaaS. Phase sau sẽ thay repository in-memory bằng PostgreSQL và event bus thật.

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

- `GET /api/sales/orders`
- `POST /api/sales/orders`
- `POST /api/sales/orders/{id}/approve`
- `POST /api/sales/orders/{id}/complete`
