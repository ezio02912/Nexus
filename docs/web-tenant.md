# Web Tenant

## Purpose

Tenant workspace for SaaS customers. The app shows only the business modules enabled on the current tenant.

## Local URL

- `http://localhost:7101`

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
