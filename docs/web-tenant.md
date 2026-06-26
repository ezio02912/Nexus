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

## CRM UX Requirements

web-tenant CRM must feel like a complete workspace, not separated demo screens.

- Tables show full important columns for each service DTO and support search/filter/sort/pagination.
- Search behavior is `lowercase + trim + contains`.
- Entity code/name cells open detail pages when available.
- Lead detail links to converted customer and opportunity.
- Customer detail links to contacts, opportunities, quotations, contracts and activities.
- Opportunity detail links to customer, source lead, quotations and contracts.
- Quotation detail links to customer, opportunity and generated contract/sales order when available.
- Contract detail links to customer, opportunity, quotation and later invoice.
- Create/edit flows support quick-create for related entities when the current workflow would otherwise block the user.
- Customer lookup supports quick-create in related forms.
- Contact lookup supports quick-create after a customer is selected in Quotation and Contract forms.
- Opportunity detail can create a linked quotation.
- Quotation detail can create a linked contract and copy quotation lines.

## Kanban Requirements

Opportunity Kanban must include:

- Stage columns with count and value summary.
- Cards with title, customer, amount, probability, owner/close date where available.
- Clear action to open detail.
- Empty state per stage.
- Responsive horizontal scroll and polished hover/focus styling.
