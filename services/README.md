# Services

Mỗi service được dựng theo cùng layout để dễ mở rộng:

```text
services/{name}-service/
├── src/
│   ├── Nexus.Services.{Name}.Api
│   ├── Nexus.Services.{Name}.Application
│   ├── Nexus.Services.{Name}.Contracts
│   ├── Nexus.Services.{Name}.Domain
│   └── Nexus.Services.{Name}.Infrastructure
├── tests/
├── migrations/
└── docs/
```

## Layer rule

- `Api`: endpoint, auth policy, request/response mapping.
- `Application`: use case, command/query handler, validation, port interfaces.
- `Domain`: aggregate, entity, value object, domain event, business rule.
- `Infrastructure`: EF Core, PostgreSQL, Redis, RabbitMQ, external clients.
- `Contracts`: service-owned API/event contracts khi cần publish rõ ràng.

## Boundary rule

- Không join database giữa services.
- Không share domain model qua `shared`.
- Giao tiếp cross-service qua REST/gateway hoặc RabbitMQ integration events.
- Mọi dữ liệu nghiệp vụ cần có `TenantId`.

## Feature completeness rule

Mọi service feature phải được làm như sản phẩm thật, không dừng ở MVP/demo.

- Domain entity, DTO, API, repository và UI phải cùng phản ánh các field nghiệp vụ chính.
- List endpoint phải hỗ trợ paging, sorting phù hợp, filter trạng thái/ngày/owner khi có và free-text search.
- Free-text search bắt buộc là `lowercase + trim + contains`.
- web-tenant table phải hiển thị đủ cột quan trọng của service. Nếu nhiều cột, dùng column chooser/responsive/detail drawer/sticky action column.
- Detail page phải có related records và link tới entity liên quan nếu service có quan hệ nghiệp vụ.
- Create/edit form phải có lookup hoặc quick-create cho entity phụ thuộc.
- Action đổi trạng thái phải xem xét permission, audit, event, workflow và notification.

## ABP-style implementation rule

Style code bám ABP Framework như `HCS_web`:

- `Domain`: rule và behavior.
- `Application`: AppService/use case, DTO mapping, event publishing.
- `Contracts`: DTO/input/output/service contracts.
- `Infrastructure`: EF Core, external clients, broker, storage.
- `Api`: endpoint mỏng.

Không đưa business rule vào endpoint hoặc Razor component.
