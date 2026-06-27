# Database Design

## Rule bắt buộc

- Mỗi service có database riêng.
- Service không join trực tiếp database của service khác.
- Dữ liệu liên thông được snapshot qua event hoặc gọi API có kiểm soát.
- Mọi bảng nghiệp vụ cần có `TenantId`.

## Naming đề xuất

| Service | Database |
| --- | --- |
| Identity | `identity_db` |
| Tenant | `tenant_db` |
| Permission | `permission_db` |
| File | `file_db` |
| Audit | `audit_db` |
| Workflow | `workflow_db` |
| Notification | `notification_db` |
| Numbering | `numbering_db` |
| Report | `report_db` |
| CRM | `crm_db` |
| ERP | `erp_db` |
| Accounting | `accounting_db` |
| HRM | `hrm_db` |
| Attendance | `attendance_db` |
| Payroll | `payroll_db` |
| Invoice | `invoice_db` |
| Inventory | `inventory_db` |
| Purchase | `purchase_db` |
| Sales | `sales_db` |

## Migration

Mỗi service giữ migration riêng trong folder `migrations` hoặc project Infrastructure khi bắt đầu code EF Core.

## Apply core migrations

```bash
cd /Users/user/Documents/Nexus
./tools/scripts/apply-core-migrations.zsh
```

Script hiện apply:

- EF migrations cho Identity, Tenant, Permission, Audit, File, Notification, Workflow và Numbering.
- `services/crm-service/migrations/*.sql` vào `crm_db`
- `services/sales-service/migrations/*.sql` vào `sales_db`
- `services/inventory-service/migrations/*.sql` vào `inventory_db`
- `services/purchase-service/migrations/*.sql` vào `purchase_db`

## Business data boundaries

- CRM giữ customer/contact/lead/opportunity/quotation/contract.
- Sales snapshot customer id và source document info từ CRM, không đọc trực tiếp database CRM.
- Inventory giữ product/warehouse/stock balance/reservation/movement. Sales và Purchase cập nhật tồn qua API Inventory.
- Purchase giữ supplier/PO/goods receipt. Khi receive PO, Purchase gọi Inventory import với source `PURCHASE_RECEIPT`.
- Accounting/Invoice giai đoạn sau sẽ đọc theo event/API source document, không join trực tiếp database Sales/Purchase.
