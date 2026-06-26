# Permission Service

## Trách nhiệm

- Quản lý permission policy.
- Gán quyền theo role, user, tenant và module.
- Cung cấp permission snapshot cho gateway/service.

## Data dự kiến

- Permissions
- RolePermissions
- UserPermissionOverrides
- ModulePermissions

## Events

- `PermissionGranted`
- `PermissionRevoked`
- `PermissionPolicyChanged`
