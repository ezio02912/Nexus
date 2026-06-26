# Identity Service

## Trách nhiệm

- Đăng nhập, refresh token, API key, OAuth2/OIDC.
- Quản lý user, role và permission context.
- Phát hành identity claims có `TenantId`.

## Data dự kiến

- Users
- Roles
- UserRoles
- RefreshTokens
- ApiKeys

## API dự kiến

- `POST /auth/login`
- `POST /auth/refresh-token`
- `GET /users`
- `POST /users`
- `GET /roles`
- `POST /roles`

## Events

- `UserCreated`
- `UserLocked`
- `RoleChanged`
