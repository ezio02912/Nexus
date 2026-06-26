# Web Tenant

## Purpose

Tenant workspace for SaaS customers. The app shows only the business modules enabled on the current tenant.

## Local URL

- `http://localhost:7101`

## Self-service onboarding (Google)

1. Configure `Google:ClientId` and `Google:ClientSecret` in `apps/web-tenant` and `identity-service` (see `.env.example`).
2. Open `/login` and choose **Đăng nhập với Google**.
3. First-time users are redirected to `/onboarding` to enter company profile and confirm tenant code.
4. After registration, an email with the tenant code is sent via Mailpit (`http://localhost:8025`).

## Login options

| Method | Fields |
|--------|--------|
| Google SSO | One click (email auto-resolves tenant) |
| Email + password | Requires password set during onboarding |
| Tenant code + username + password | Classic demo/admin-style login |

Constraint: one email can register one tenant.

## Demo Login

- Tenant code: `DEMO`
- User name: `tenantadmin`
- Password: `123123`
- Demo tenant modules: `CRM`, `SALES`

## Module Visibility

- Tenant modules are loaded from Tenant Service after login.
- CRM menu appears only when module `CRM` is enabled.
- Sales menu appears only when module `SALES` is enabled.
- Tenant admin pages are visible after login: Users, Permissions, Workflow, Reports, Settings.

## Tenant Admin

- Users are created with the current `TenantId`.
- Permissions are saved with a scoped role key: `{tenantId:N}:{roleName}`.
- Permission catalog includes platform, CRM and Sales permissions:
  - `Nexus.Crm.*`
  - `Nexus.Sales.*`

## BootstrapBlazor UI

- Uses BootstrapBlazor from `external/bootstrap-blazor`.
- Tenant style follows Ocean Enterprise SaaS rules in `.cursor/rules/ui-bootstrapblazor-saas-rule.mdc`.
