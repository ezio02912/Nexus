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

- `services/tenant-service/migrations/*.sql` vào `tenant_db`
- `services/identity-service/migrations/*.sql` vào `identity_db`
