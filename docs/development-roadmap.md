# Development Roadmap

Roadmap này dùng cho cả người phát triển và AI coding agent. Mục tiêu là xây sản phẩm SaaS enterprise đầy đủ theo service, không làm màn MVP/demo chỉ có vài field.

## Nguyên tắc phase

- Mỗi feature phải đi đủ vòng: Domain, Application contract, API endpoint, EF mapping/migration, permission, audit, docs, web-tenant UI, table filter, detail page, quick-create/link liên quan, test tối thiểu.
- Mỗi list table phải có đủ các cột nghiệp vụ chính theo DTO/domain, không được chỉ hiển thị 3-4 cột demo. Với nhiều cột, dùng column chooser, responsive priority, sticky action column hoặc detail drawer.
- Search/filter ở table và API phải dùng chuẩn `lowercase + trim + contains`.
- UX web-tenant phải thể hiện luồng liên thông dữ liệu: Lead -> Customer -> Opportunity -> Quotation -> Contract -> Sales/Invoice/Accounting.
- Style backend/frontend bám ABP Framework style từ `HCS_web`: module rõ lớp, AppService mỏng, DTO rõ, permission/audit đầy đủ, code-behind cho Razor, naming nhất quán, không đẩy business rule vào endpoint.

## Phase 0: Foundation Hardening

Trạng thái: cần hoàn thiện trước khi mở rộng nhiều module.

- Chuẩn hóa solution/module/service conventions theo ABP-style: Domain, Application, Contracts, Infrastructure, Api.
- Hoàn thiện shared building blocks: repository, unit of work, audit, current user/tenant, correlation, event outbox/inbox.
- Chuẩn hóa auth, permission catalog, tenant context, service-to-service auth.
- Chuẩn hóa logging: mọi request/event có `TenantId`, `UserId`, `CorrelationId`, service name.
- Hoàn thiện local development stack: PostgreSQL, Redis, RabbitMQ, MinIO, Mailpit, migration, seed data.
- Tạo checklist Definition of Done cho mọi feature.

Kết quả cần có:

- `dotnet build` chạy được toàn repo.
- Seed data đủ để demo tenant có CRM/Sales và tài khoản tenant admin.
- Docs local development phản ánh đúng lệnh chạy hiện tại.

## Phase 1: Core Platform Services

Phạm vi:

- Identity Service: login, refresh token, users, roles, user-role, external login, onboarding.
- Tenant Service: tenant profile, subscription, module enablement, quotas, billing state.
- Permission Service: role permission, feature permission, tenant-scoped role key.
- Audit Service: audit log, entity change, user access log.
- File Service: upload/download, file version, linked attachments, virus-scan placeholder.
- Notification Service: email, in-app, webhook, template.
- Workflow Service: approval definition, approval instance, task/action history.
- Numbering Service: document sequence by tenant/module/document type/period.

Yêu cầu UI:

- web-admin là control center cho platform owner.
- web-tenant chỉ hiện module tenant được bật.
- Bảng Tenants, Users, Roles, Audit Logs, Service Health phải có full columns, advanced filters và action rõ.

## Phase 2: CRM Enterprise Flow

Phạm vi:

- Customer: profile đầy đủ, owner, rating, source, address, tax, contact, activity timeline.
- Contact: customer link, primary/decision-maker, department, position, notes.
- Lead: source, score, rating, status, assignment, lost reason, convert flow.
- Opportunity: customer/lead link, pipeline stage, amount, probability, close date, close/lost reason.
- Quotation: header, lines, discount/tax, status, approval, PDF/export.
- Contract: header, lines, sign/active/expire/terminate flow.
- Activity: call/email/meeting/task/note gắn với customer/lead/opportunity/quotation/contract.

Yêu cầu liên thông:

- Lead detail có action convert tạo Customer + Opportunity và link tới bản ghi mới.
- Customer detail hiển thị contacts, opportunities, quotations, contracts, activities.
- Opportunity detail click tới customer, lead nguồn, quotation/contract liên quan.
- Các màn cần tạo nhanh Customer/Contact/Opportunity khi đang nhập dữ liệu liên quan.
- Kanban opportunity phải có UI hoàn chỉnh: stage summary, amount total, probability, owner, due/close date, empty state, smooth responsive scroll, action nhanh và link detail.

## Phase 3: Sales + Inventory + Purchase

Phạm vi:

- Sales order: customer, quotation source, order lines, discount/tax, delivery status.
- Product catalog: SKU, unit, category, price, tax, active state.
- Warehouse: warehouse, location, stock balance, stock movement.
- Stock in/out/transfer and inventory adjustment.
- Supplier, purchase request, purchase order, goods receipt.

Yêu cầu liên thông:

- Quotation -> Sales Order.
- Sales Order -> Stock reservation/delivery.
- Purchase Order -> Goods Receipt -> Stock movement.
- Product detail hiển thị tồn kho và lịch sử movement.

Tiến độ hiện tại:

- CRM Quotation/Contract detail đã có nút tạo Sales Order và prefill Sales Order form qua web-tenant.
- Sales Order backend đã có source document fields tối thiểu (`SourceType`, `SourceId`, `SourceNo`) để trace về Quotation/Contract.
- Sales Order đã có detail page, trạng thái giữ hàng/giao hàng và link ngược CRM.
- Inventory Service đã có stock balance, manual import, reservation và shipment API.
- Inventory Service đã có product catalog và warehouse catalog nền tảng, kèm UI tenant tách riêng `/inventory`, `/inventory/products`, `/inventory/warehouses`.
- Sales Order approve gọi Inventory reservation; deliver/complete gọi Inventory shipment.
- Sales Order đã có pricing/discount/tax trên line và tổng tiền, đồng thời form tạo đơn có thể chọn product từ Inventory catalog.
- Sales Order form đã hỗ trợ nhiều dòng hàng, chọn warehouse theo từng dòng và truyền warehouse sang Inventory reservation/shipment.
- Purchase Service đã có supplier, purchase order, approve và goods receipt; receive PO gọi Inventory stock import với source `PURCHASE_RECEIPT`.
- Tenant web `/purchase` đã có tab đơn mua, nhà cung cấp và phiếu nhận.
- Bước tiếp theo của Phase 3 là invoice từ Sales Order và payable từ Purchase.

## Phase 4: Invoice + Accounting Core

Phạm vi:

- Invoice lifecycle, invoice lines, tax, payment state, e-invoice integration placeholder.
- Payment receipt/payment voucher.
- Receivable/payable.
- Chart of accounts, journal entry, posting rules.
- Accounting events từ Sales, Purchase, Payroll.

Yêu cầu liên thông:

- Sales Order/Contract -> Invoice.
- Invoice -> Receivable -> Payment.
- Purchase -> Payable -> Payment.
- Mọi accounting entry trace được về source document.

## Phase 5: HRM + Attendance + Payroll

Phạm vi:

- Employee profile, department, position, labor contract.
- Shift, check-in/out, leave, overtime.
- Payroll period, payroll items, calculation, approval, payslip.
- Payroll posting event sang Accounting.

Yêu cầu liên thông:

- Employee detail hiển thị contract, attendance, leave, payroll history.
- Payroll batch có workflow approval và audit trail.

## Phase 6: Reports + Operational Excellence

Phạm vi:

- Dashboard/read model cho CRM, Sales, Inventory, Accounting, HRM, Payroll.
- Reports: pipeline, sales, receivable/payable, inventory, ledger, payroll.
- Audit trace xuyên service.
- Observability: health checks, metrics, background jobs, failed event replay.

Yêu cầu:

- Report filter phải dùng cùng chuẩn `lowercase + trim + contains` cho search text.
- Export CSV/XLSX/PDF cho các bảng chính.
- Dashboard có drill-down tới bản ghi nguồn.

## Definition of Done cho mọi feature

- Contract/DTO có đủ field nghiệp vụ chính, không thiếu field so với domain entity.
- API list/detail/create/update/delete hoặc action nghiệp vụ chính đã có.
- Table hiển thị đủ cột quan trọng, có search, filter, sort, pagination, empty/loading/error state.
- Detail page có section thông tin chính, audit metadata, related records và action liên thông.
- Create/update form có validation, master data lookup, quick-create nếu liên quan đến entity khác.
- Permission check có ở API và UI.
- Audit/event được ghi cho action thay đổi trạng thái quan trọng.
- Test hoặc verification tối thiểu được ghi lại trong docs/PR.

## Đề xuất bước tiếp theo

1. Bắt đầu Phase 4 bằng Invoice từ Sales Order/Contract, có invoice lines, tax, payment state và trace source document.
2. Thêm Supplier Invoice/Payable từ Goods Receipt để khép luồng Purchase -> Payable.
3. Bổ sung Accounting posting rules tối thiểu cho Sales Invoice, Supplier Invoice, Payment receipt/payment voucher.
4. Hoàn thiện Inventory product detail: tồn theo warehouse, reserved/available và lịch sử stock movement theo source document.
5. Viết seed scenario end-to-end: Lead -> Customer -> Opportunity -> Quotation -> Contract -> Sales Order -> Inventory shipment -> Invoice -> Payment; Supplier -> PO -> Goods Receipt -> Supplier Invoice -> Payment.
6. Bổ sung test cho service domain và API quan trọng: Sales pricing/reservation, Inventory reserve/ship/import, Purchase receive PO.
