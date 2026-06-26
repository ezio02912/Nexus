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
