# CRM Service

## Trách nhiệm

- Khách hàng, liên hệ, lead, cơ hội bán hàng.
- Báo giá, hợp đồng và lịch sử chăm sóc.

## Data hiện tại

- Customers
- Contacts
- Leads
- Opportunities
- Quotations
- Contracts

## API hiện tại

- `GET /api/crm/customers?tenantId={tenantId}`
- `POST /api/crm/customers`
- `GET /api/crm/leads?tenantId={tenantId}`
- `POST /api/crm/leads`
- `GET /api/crm/opportunities?tenantId={tenantId}`
- `POST /api/crm/opportunities`
- `GET /api/crm/quotations?tenantId={tenantId}`
- `POST /api/crm/quotations`
- `POST /api/crm/quotations/{id}/approve`
- `GET /api/crm/contracts?tenantId={tenantId}`
- `POST /api/crm/contracts`
- `POST /api/crm/contracts/{id}/sign`

## Migration

- SQL migration: `services/crm-service/migrations/001_create_crm_core.sql`
- Database: `crm_db`
- Local port: `http://localhost:7208`
- Runtime persistence: PostgreSQL via EF Core `CrmDbContext`

## Tenant Web

- Menu CRM chỉ hiển thị khi Tenant Service bật module `CRM`.
- Tenant admin có thể phân quyền theo catalog `Nexus.Crm.*` qua Permission page.

## Events

- `CustomerCreated`
- `QuotationApproved`
- `ContractSigned`
