# CRM Service

## Trách nhiệm

- Khách hàng, liên hệ, lead, cơ hội bán hàng.
- Báo giá (header + dòng hàng), hợp đồng và lịch sử chăm sóc (activities).
- Dashboard KPI pipeline.

## Kiến trúc

```
services/crm-service/src/
  Nexus.Services.Crm.Domain/
  Nexus.Services.Crm.Contracts/
  Nexus.Services.Crm.Application/
  Nexus.Services.Crm.Infrastructure/
  Nexus.Services.Crm.Api/
```

## Data

| Entity | Bảng | Ghi chú |
|--------|------|---------|
| Customer | `customers` | Full audit, địa chỉ, MST, owner, rating |
| Contact | `contacts` | Liên kết customer, primary/decision maker |
| Lead | `leads` | Score, convert workflow |
| Opportunity | `opportunities` | Pipeline stage, probability, currency |
| Quotation | `quotations` + `quotation_lines` | Approve/reject/send |
| Contract | `contracts` + `contract_lines` | Sign/activate/terminate, file_id |
| Activity | `crm_activities` | Call/email/meeting/task/note |
| PipelineStage | `pipeline_stages` | Cấu hình Kanban |

## API (tenant context qua JWT / `x-tenant-id`)

Mọi endpoint yêu cầu `[Authorize]` và permission `Nexus.Crm.*`.

### CRUD

- `/api/crm/customers`, `/contacts`, `/leads`, `/opportunities`, `/quotations`, `/contracts`, `/activities`
- Pattern: `GET` list (paging), `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}`

### Workflow

- `POST /api/crm/leads/{id}/convert`
- `PATCH /api/crm/opportunities/{id}/stage`
- `POST /api/crm/quotations/{id}/approve|reject|send`
- `POST /api/crm/contracts/{id}/sign|activate|terminate`
- `POST /api/crm/activities/{id}/complete`
- `GET /api/crm/dashboard`

## Migration

- SQL: `001_create_crm_core.sql`, `002_expand_crm_enterprise.sql`
- EF Core: `Nexus.Services.Crm.Infrastructure/Migrations/`
- Database: `crm_db`
- Local port: `http://localhost:7208`

## Tích hợp

- **Numbering Service**: auto `QT-` / `CT-` khi tạo báo giá/hợp đồng với số `AUTO`
- **File Service**: `file_id` trên contract (attachment PDF)
- **Events** (outbox): `CustomerCreated`, `LeadConverted`, `OpportunityStageChanged`, `QuotationApproved`, `QuotationRejected`, `ContractSigned`

## Permissions

- `Nexus.Crm.Customers`, `.Contacts`, `.Leads`, `.Opportunities`, `.Quotations`, `.Contracts`, `.Activities`

## Tenant Web

- Menu CRM: Dashboard, Khách hàng, Liên hệ, Leads, Cơ hội, Kanban, Báo giá, Hợp đồng, Hoạt động
- Client: `apps/web-tenant/.../Services/CrmApiClient.cs`

## UX Completion Checklist

- Leads table/detail phải hiển thị và cho sửa các field chính: full name, company, title, email, phone, source, score, rating, status, owner, converted customer/opportunity, lost reason, address, audit dates.
- Customers table/detail phải có code, name, type, email, phone, tax code, website, industry, full address, owner, rating, source, status và related contacts/opportunities/quotations/contracts/activities.
- Opportunities table/detail/Kanban phải có customer/lead/contact link, name, stage, amount, probability, currency, expected/actual close date, next step, source, owner, reason fields.
- Quotations và Contracts phải có header + lines, status workflow, amount fields, customer/opportunity/contact links và source document trace.
- Search của tất cả list CRM phải dùng `lowercase + trim + contains`.
- Kanban phải có stage count, value summary, card metadata, empty state, detail action và chuyển stage rõ ràng.
- Opportunity detail phải tạo nhanh được Quotation với link `OpportunityId`.
- Quotation detail phải tạo nhanh được Contract với link `QuotationId` và copy lines từ báo giá.
- Quotation/Contract create forms phải chọn hoặc tạo nhanh được Contact theo Customer đã chọn.
