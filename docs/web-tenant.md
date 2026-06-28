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
- Demo tenant modules tối thiểu: `CRM`, `SALES`; các phase mới dùng thêm `INVENTORY`, `PURCHASE` khi tenant được bật module tương ứng.

## Module Visibility

- Tenant modules are loaded from Tenant Service after login.
- CRM menu appears only when module `CRM` is enabled.
- Sales menu appears only when module `SALES` is enabled.
- Inventory menu appears only when module `INVENTORY` is enabled.
- Purchase menu appears only when module `PURCHASE` is enabled.
- Tenant admin pages are visible after login: Users, Permissions, Workflow, Reports, Settings.

## Tenant Admin

- Users are created with the current `TenantId`.
- Permissions are saved with a scoped role key: `{tenantId:N}:{roleName}`.
- Permission catalog includes platform and business permissions:
  - `Nexus.Crm.*`
  - `Nexus.Sales.*`
  - `Nexus.Inventory.*`
  - `Nexus.Purchase.*`
  - `Nexus.Files.View`, `Nexus.Files.Upload`, `Nexus.Files.Delete`

## BootstrapBlazor UI

- Uses BootstrapBlazor from `external/bootstrap-blazor`.
- Tenant style follows Ocean Enterprise SaaS rules in `.cursor/rules/ui-bootstrapblazor-saas-rule.mdc`.
- Money fields use `AppMoneyInput` for realtime Vietnamese thousands/decimal formatting. `BootstrapInputNumber` is reserved for quantities, percentages, days, hours and non-money numeric fields.

## CRM UX Requirements

web-tenant CRM must feel like a complete workspace, not separated demo screens.

- Tables show full important columns for each service DTO and support search/filter/sort/pagination.
- Search behavior is `lowercase + trim + contains`.
- Entity code/name cells open detail pages when available.
- Lead detail links to converted customer and opportunity.
- Customer detail links to contacts, opportunities, quotations, contracts and activities.
- Activities page uses a calendar workspace; related entity selection is driven by the selected entity type instead of manual GUID input, and activity assignment supports multiple responsible users.
- Opportunity detail links to customer, source lead, quotations and contracts.
- Quotation detail links to customer, opportunity and generated contract/sales order when available.
- Contract detail links to customer, opportunity, quotation and later invoice.
- Create/edit flows support quick-create for related entities when the current workflow would otherwise block the user.
- Customer lookup supports quick-create in related forms.
- Contact lookup supports quick-create after a customer is selected in Quotation and Contract forms.
- Opportunity detail can create a linked quotation.
- Opportunity detail quotation creation uses the tenant product lookup and supports multiple product lines.
- Quotation detail can create a linked contract and copy quotation lines.
- Quotation and Contract create forms can select attachment files; files are uploaded after the entity is created and linked through File Service.
- Contract creation captures start date, end date, renewal date, payment terms, notes and terms so the initial contract is not a thin placeholder.
- Contract workflow includes completed status; completed contracts are read-only in tenant UI and lock upload/delete actions for attachments.

## Sales / Inventory / Purchase Workflow

web-tenant hiện hỗ trợ các workflow nghiệp vụ chính của phase 3:

- Quotation hoặc Contract detail có action tạo Sales Order và prefill khách hàng, chứng từ nguồn, dòng hàng, thuế và chiết khấu.
- Sales Orders page tạo được nhiều dòng hàng, chọn product từ Inventory catalog và chọn warehouse theo từng dòng.
- Sales Orders page tạo đơn bằng dialog; danh sách chỉ còn bảng và action, không đặt form tạo mới cố định cạnh bảng.
- Sales Order approve gọi Inventory reservation; deliver/complete gọi Inventory shipment.
- Sales Order detail hiển thị source CRM, trạng thái giữ hàng/giao hàng, subtotal, discount, tax và line warehouse.
- Inventory tách page riêng cho tồn kho, mã hàng hoá và kho hàng; nhập kho nhanh tự tạo product/warehouse catalog nếu chưa tồn tại.
- Warehouse catalog có tuỳ chọn `Cho phép tồn kho âm`; khi bật, giữ hàng/chuyển kho có thể đưa tồn khả dụng xuống âm để xử lý case chờ hàng về.
- Tenant menu exposes direct pages for `Tồn kho`, `Mã hàng hoá` and `Kho hàng`.
- `Mã hàng hoá` routes to `/inventory/products`; `Kho hàng` routes to `/inventory/warehouses`.
- Product catalog uses Master Data lookups for unit (`Unit`) and product type (`ProductType`), supports product images/files through File Service, and stores attributes/variants on the Inventory catalog.
- Create/edit actions for Inventory, Purchase, Supplier and Tenant User lists open BootstrapBlazor dialogs instead of keeping add forms beside the list.
- Purchase tách page riêng cho đơn mua, nhà cung cấp và phiếu nhận.
- Purchase Order form chọn nhà cung cấp từ danh mục supplier lookup/select thay vì nhập mã NCC tay.
- Menu Mua hàng có lối vào riêng cho `Đơn mua`, `Nhà cung cấp` và `Phiếu nhận hàng`.
- `Phiếu nhận hàng` trỏ tới `/purchase/receipts`, không dùng `?tab=receipts`.
- Purchase Order approve chuyển trạng thái sang đã duyệt; receive tạo Goods Receipt và gọi Inventory stock import theo source `PURCHASE_RECEIPT`.
- Phiếu nhận Purchase làm tăng tồn kho, sau đó Inventory balances phản ánh số lượng mới.

## Kanban Requirements

Opportunity Kanban must include:

- Stage columns with count and value summary.
- Cards with title, customer, amount, probability, owner/close date where available.
- Clear action to open detail.
- Empty state per stage.
- Responsive horizontal scroll and polished hover/focus styling.
