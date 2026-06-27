# Service Map

## Core Platform

- Identity Service: đăng nhập, refresh token, user, role, permission context.
- Tenant Service: tenant, subscription, module, setting, quota.
- Permission Service: policy, role permission, feature permission.
- File Service: upload, download, version, file links.
- Audit Service: audit log, entity change, user access log.
- Workflow Service: approval definition, instance, action.
- Notification Service: email, in-app notification, webhook.
- Numbering Service: document number sequence.

## Business Domain

- CRM Service: customer, contact, lead, opportunity, quotation, contract.
- Sales Service: sales order, source trace từ CRM, line pricing/discount/tax, stock reservation/delivery.
- Inventory Service: product catalog, warehouse catalog, stock balance, stock reservation, shipment, stock movement.
- API Gateway exposes Inventory at `/inventory/**` and Purchase at `/purchase/**`; Notification runs on port `7213` so Inventory owns `7210`.
- Purchase Service: supplier, purchase order, approval, goods receipt, stock import integration.
- Invoice Service: invoice lifecycle, e-invoice integration, invoice file.
- Accounting Service: chart of accounts, journal, receivable, payable, payment.
- HRM Service: employee, department, position, labor contract.
- Attendance Service: shift, check-in/out, leave, overtime.
- Payroll Service: payroll period, calculation, approval, payment.
- ERP Service: umbrella/integration boundary for ERP workflows that do not belong to a narrower service yet.
- Report Service: read models and reports.

## Tenant Runtime Workflow

- Tenant Service quyết định module nào bật cho tenant.
- Permission Service quyết định menu/action nào user được thấy trong module đã bật.
- web-tenant chỉ hiển thị module và menu con thỏa cả hai điều kiện trên.
- Business services luôn nhận `TenantId` từ request/query/header và không join database chéo service.
- Luồng liên thông hiện có: CRM Quotation/Contract -> Sales Order -> Inventory reservation/shipment, và Purchase Order -> Goods Receipt -> Inventory stock import.
