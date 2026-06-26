# Identity Service

## Trách nhiệm

- Đăng nhập, refresh token, Google OAuth, onboarding tenant self-service.
- Quản lý user, role và permission context.
- Phát hành identity claims có `TenantId`.

## Data

- Users
- UserRoles
- RefreshTokens
- ExternalLogins
- OnboardingSessions
- TenantRegistrations (1 email → 1 tenant)

## Auth API

| Endpoint | Mô tả |
|----------|-------|
| `POST /api/auth/login` | TenantId + username + password |
| `POST /api/auth/login-email` | Email + password (auto-resolve tenant) |
| `POST /api/auth/google` | Google ID token → JWT hoặc onboarding token |
| `POST /api/auth/refresh` | Refresh access token |
| `GET /api/auth/me/tenant?email=` | Tra tenant theo email |

## Onboarding API

| Endpoint | Mô tả |
|----------|-------|
| `POST /api/onboarding/preview-code` | Gợi ý mã tenant từ tên công ty |
| `POST /api/onboarding/complete` | Tạo tenant + TENANTADMIN user |

## User API

- `GET /api/users`
- `POST /api/users`
- `POST /api/users/{id}/roles`

## Events

- `UserCreated`
- `UserRoleChanged`

## Config

- `Google:ClientId` — validate Google ID tokens
- `Services:Tenant` — orchestration tạo tenant qua internal API
- `Internal:ApiKey` — gọi `POST /api/internal/tenants`
- `Onboarding:DefaultModules` — modules bật mặc định khi đăng ký
