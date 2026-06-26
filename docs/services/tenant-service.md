# Tenant Service

## Trách nhiệm

- Quản lý công ty thuê hệ thống.
- Gói dịch vụ, module, quota, trạng thái hoạt động.
- Cấu hình tenant.

## Data dự kiến

- Tenants
- TenantSubscriptions
- TenantModules
- TenantSettings

## API dự kiến

- `GET /tenants`
- `POST /tenants`
- `GET /tenants/{id}`
- `PUT /tenants/{id}/settings`

## Events

- `TenantCreated`
- `TenantActivated`
- `TenantSuspended`
