# Coding Standards

## ABP-like Architecture

Code style của Nexus phải bám cách tổ chức quen thuộc trong ABP Framework và tham chiếu `HCS_web`:

- Domain chứa entity, value object, enum, domain rule, repository interface, domain event nếu cần.
- Application chứa AppService/use case, DTO mapping, validation orchestration và interface ports.
- Application Contracts chứa DTO/input/output/service contracts dùng qua service boundary.
- Infrastructure chứa EF Core repository, DbContext configuration, external clients, broker, file store.
- Api chỉ expose endpoint, auth policy, request binding và orchestration nhẹ.
- Web UI dùng component/service client riêng, không gọi trực tiếp DbContext hay nhét business rule vào Razor.

Không dùng shared library để chia sẻ domain model giữa service. Shared chỉ chứa building blocks, primitive contract hoặc cross-cutting concern.

## Feature Completeness

Không tạo feature kiểu MVP/demo.

Mỗi feature phải có:

- Entity/domain model đủ field nghiệp vụ chính.
- DTO list/detail/create/update/action không thiếu field quan trọng so với domain.
- API list/detail/create/update/delete hoặc action nghiệp vụ chính.
- Permission, audit metadata, tenant isolation và correlation.
- List table đủ cột chính, có search/filter/sort/pagination/loading/empty/error state.
- Detail page có related records và link tới entity liên quan.
- Form có validation, lookup/master data, quick-create khi field tham chiếu entity khác.
- Docs cập nhật trong `docs/services` hoặc roadmap khi scope thay đổi.

## Search and Table Filtering

Mọi search text trong table/list API phải dùng chuẩn:

```text
term = input.Trim().ToLowerInvariant()
match = field.ToLower().Contains(term)
```

Áp dụng cho repository EF Core, in-memory query, service query và report query. Không dùng `StartsWith`, exact match hoặc case-sensitive `Contains` cho ô search tổng trừ khi nghiệp vụ ghi rõ khác.

Quy tắc:

- Trim input trước khi query.
- Lowercase input và các field text so sánh.
- Search nhiều field có ý nghĩa, ví dụ code/name/email/phone/title.
- Nếu dùng PostgreSQL-specific implementation sau này, có thể đổi sang `ILIKE '%term%'` nhưng behavior phải giữ lowercase-trim-contains.
- Filter enum/status/date/range tách khỏi search text.

## Backend Clean Code

- Method ngắn, tên rõ nghĩa, ưu tiên AppService mỏng và domain method có nghĩa.
- Không nhét business rule vào controller/endpoint.
- Tách command/query khi use case lớn hoặc có workflow.
- Log có `TenantId`, `CorrelationId`, `UserId`.
- Không swallow exception; lỗi nghiệp vụ dùng exception/code rõ.
- Normalize input trong domain hoặc application theo một nơi nhất quán.
- Các action đổi trạng thái phải có audit/event khi có ý nghĩa nghiệp vụ.

## Frontend Standards

- Blazor + BootstrapBlazor + C# + Razor code-behind `.razor.cs`.
- Không dùng raw HTML table khi BootstrapBlazor Table đáp ứng được.
- Không hardcode màu trong Razor; dùng CSS variables/token.
- Page list không bọc card lồng card; table có toolbar, search, column/action rõ.
- Các màn liên quan phải link được với nhau. Ví dụ CRM: Lead -> Customer -> Opportunity -> Quotation -> Contract.
- Form tạo mới ở entity phụ thuộc phải hỗ trợ quick-create khi workflow cần, ví dụ tạo nhanh Customer khi tạo Opportunity.
- Kanban phải có stage summary, card metadata đủ, empty state, responsive horizontal scroll, hover/focus state và action detail.

## Testing

- Unit test cho domain rules.
- Integration test cho repository, API, event handler.
- Contract test cho event/API quan trọng.
- Search/filter test phải cover case khác hoa/thường, khoảng trắng đầu/cuối và contains ở giữa chuỗi.
