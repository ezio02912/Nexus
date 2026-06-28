# Development Roadmap

Roadmap này dùng cho cả người phát triển và AI coding agent. Mục tiêu là xây sản phẩm SaaS enterprise đầy đủ theo service, không làm màn MVP/demo chỉ có vài field.

## Nguyên tắc phase

- Mỗi feature phải đi đủ vòng: Domain, Application contract, API endpoint, EF mapping/migration, permission, audit, docs, web-tenant UI, table filter, detail page, quick-create/link liên quan, test tối thiểu.
- Mỗi list table phải có đủ các cột nghiệp vụ chính theo DTO/domain, không được chỉ hiển thị 3-4 cột demo. Với nhiều cột, dùng column chooser, responsive priority, sticky action column hoặc detail drawer.
- Search/filter ở table và API phải dùng chuẩn `lowercase + trim + contains`.
- UX web-tenant phải thể hiện luồng liên thông dữ liệu: Lead -> Customer -> Opportunity -> Quotation -> Contract -> Sales/Inventory/Purchase trước; Invoice/Accounting để phase cuối.
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
- Contract: header, lines, sign/active/complete/expire/terminate flow.
- Activity: call/email/meeting/task/note gắn với customer/lead/opportunity/quotation/contract, hiển thị dạng calendar và hỗ trợ nhiều người phụ trách.

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
- Warehouse: warehouse, location, stock balance, stock movement, cấu hình cho phép tồn kho âm theo kho.
- Stock in/out/transfer and inventory adjustment.
- Supplier, purchase request, purchase order, goods receipt.

Yêu cầu liên thông:

- Quotation -> Sales Order.
- Sales Order -> Stock reservation/delivery.
- Purchase Order -> Goods Receipt -> Stock movement.
- Product detail hiển thị tồn kho và lịch sử movement.

Tiến độ hiện tại:

- CRM Quotation/Contract detail đã có nút tạo Sales Order và prefill Sales Order form qua web-tenant.
- Contract đã có trạng thái hoàn thành; hợp đồng hoàn thành bị khóa sửa/xóa và khóa thao tác upload/xóa đính kèm ở tenant UI.
- Sales Order backend đã có source document fields tối thiểu (`SourceType`, `SourceId`, `SourceNo`) để trace về Quotation/Contract.
- Sales Order đã có detail page, trạng thái giữ hàng/giao hàng và link ngược CRM.
- Inventory Service đã có stock balance, manual import, reservation và shipment API.
- Inventory Service đã có product catalog và warehouse catalog nền tảng, kèm UI tenant tách riêng `/inventory`, `/inventory/products`, `/inventory/warehouses`.
- Warehouse catalog đã có tuỳ chọn cho phép tồn kho âm; Inventory reservation/transfer cho phép âm chỉ khi kho nguồn bật cấu hình này.
- Sales Order approve gọi Inventory reservation; deliver/complete gọi Inventory shipment.
- Sales Order đã có pricing/discount/tax trên line và tổng tiền, đồng thời form tạo đơn có thể chọn product từ Inventory catalog.
- Sales Order form đã hỗ trợ nhiều dòng hàng, chọn warehouse theo từng dòng và truyền warehouse sang Inventory reservation/shipment.
- Purchase Service đã có supplier, purchase order, approve và goods receipt; receive PO gọi Inventory stock import với source `PURCHASE_RECEIPT`.
- Tenant web `/purchase` đã có tab đơn mua, nhà cung cấp và phiếu nhận.
- Bước tiếp theo của Phase 3 là hoàn thiện Inventory detail, cảnh báo tồn âm và seed/test vận hành CRM/Sales/Inventory/Purchase trước khi sang Invoice.

## Phase 4: HRM + Attendance + Payroll

Phạm vi:

- HRM đầy đủ: dashboard, nhân viên, phòng ban, chức vụ, hợp đồng lao động, hồ sơ, lịch sử nhân sự, tuyển dụng, onboarding/offboarding.
- Attendance đầy đủ: lịch làm việc, ngày nghỉ, ca làm, phân ca, chấm công, sửa công, nghỉ phép, số dư phép, tăng ca, bảng công.
- Payroll đầy đủ theo Việt Nam: policy hiệu lực ngày, BHXH/BHYT/BHTN, công đoàn, giảm trừ gia cảnh, thuế TNCN lũy tiến, phụ cấp/khấu trừ, kỳ lương, bảng lương, phiếu lương, chi lương.
- Mặc định Việt Nam: T2-T6, 08:00-17:00, nghỉ trưa 12:00-13:00; phép năm 12 ngày/năm, cho phép cấu hình theo policy/nhóm lao động.
- Payroll phase này không sinh bút toán kế toán; chỉ giữ snapshot/event để Accounting ở Final Phase consume.

Yêu cầu liên thông:

- Employee detail hiển thị contract, attendance, leave, payroll history.
- Payroll batch có workflow approval và audit trail.
- Offer accepted tạo Employee draft và onboarding checklist.
- Leave approved trừ số dư phép; rejected/cancelled không trừ phép.
- Payroll run lấy snapshot bảng công, nghỉ phép, tăng ca và thành phần lương để tính gross, bảo hiểm, taxable income, TNCN và net pay.

Tiến độ hiện tại:

- Đã thêm `hrm-service`, `attendance-service`, `payroll-service` vào build/run scripts.
- Đã thêm migration SQL nền tảng cho HRM, Attendance, Payroll.
- Đã thêm permission groups `Nexus.Hrm.*`, `Nexus.Attendance.*`, `Nexus.Payroll.*`.
- Đã thêm menu tenant cho HRM, Attendance và Payroll theo từng nghiệp vụ.
- Đã thêm API CRUD/list/action chính cho ba service và màn hình web-tenant list/filter/pagination/action nhanh.


## Phase 5: Invoice Core

Phạm vi:

- Invoice lifecycle, invoice lines, tax, payment state, e-invoice integration placeholder.
- Payment receipt/payment voucher.
- Receivable/payable.
- Trace source document từ Sales/Contract/Purchase để chuẩn bị posting kế toán ở phase cuối.

Yêu cầu liên thông:

- Sales Order/Contract -> Invoice.
- Invoice -> Receivable -> Payment.
- Purchase -> Payable -> Payment.
- Dữ liệu invoice/payable phải đủ snapshot để phase kế toán cuối có thể posting không join database chéo.


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

## Final Phase: Accounting Core

Phạm vi:

- Hệ thống tài khoản, kỳ kế toán, bút toán, posting rules.
- Accounting events từ Sales, Purchase, Invoice và Payroll.
- Receivable/payable/payment posting dựa trên snapshot chứng từ.
- Mọi accounting entry trace được về source document.

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

1. Hoàn thiện Inventory product detail: tồn theo warehouse, reserved/available, cảnh báo tồn âm và lịch sử stock movement theo source document.
2. Hoàn thiện CRM Activities calendar: filter theo loại hoạt động/trạng thái/người phụ trách và drag/drop đổi ngày nếu BootstrapBlazor hỗ trợ ổn định.
3. Viết seed scenario vận hành chưa kế toán: Lead -> Customer -> Opportunity -> Quotation -> Contract -> Sales Order -> Inventory reservation/shipment; Supplier -> PO -> Goods Receipt -> Stock Import.
4. Bổ sung test cho service domain và API quan trọng: Sales pricing/reservation, Inventory reserve/ship/import/negative-stock, Purchase receive PO.
5. Sau khi CRM/Sales/Inventory/Purchase/HRM ổn định mới bắt đầu Invoice, và Accounting để phase cuối.
