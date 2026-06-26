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

- `GET /api/crm/customers`
- `POST /api/crm/customers`
- `GET /api/crm/leads`
- `POST /api/crm/leads`
- `GET /api/crm/opportunities`
- `POST /api/crm/opportunities`
- `GET /api/crm/quotations`
- `POST /api/crm/quotations`
- `POST /api/crm/quotations/{id}/approve`
- `GET /api/crm/contracts`
- `POST /api/crm/contracts`
- `POST /api/crm/contracts/{id}/sign`

## Sales Service

- `GET /api/sales/orders`
- `POST /api/sales/orders`
- `POST /api/sales/orders/{id}/approve`
- `POST /api/sales/orders/{id}/complete`
