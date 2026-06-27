# Architecture

## Tổng quan

Nexus SaaS Platform dùng monorepo để AI agent và code team dễ phát triển theo từng phase. Backend đi theo microservices, mỗi service có boundary, database, migration, log và health check riêng.

```text
Blazor Web / Flutter Mobile
        |
        v
API Gateway / BFF
        |
        +--> Core Services
        +--> Business Services
        +--> Reporting Services
        |
        v
RabbitMQ Event Bus
        |
        v
Background Workers
```

## Backend

- Runtime: .NET 10.
- Style: Clean Architecture.
- API: ASP.NET Core Minimal API/Web API.
- Data: PostgreSQL, database riêng theo service.
- Cache: Redis.
- Messaging: RabbitMQ.
- Worker: .NET Worker host.

## Frontend

- `apps/web-admin`: Blazor cho vận hành platform/admin.
- `apps/web-tenant`: Blazor cho tenant user.
- `external/bootstrap-blazor`: source BootstrapBlazor dùng làm UI framework source/vendor.
- Tenant workflow hiện tại được mô tả tại `docs/tenant-workflow.md`.

## Mobile

`apps/mobile` là placeholder Flutter. Chưa tạo app code để tránh khóa kiến trúc mobile quá sớm.

## Shared

- `shared/common-kernel`: primitive, base entity, tenant context.
- `shared/building-blocks`: cross-cutting helpers, event bus abstraction, audit abstraction.
- `shared/event-contracts`: integration events.
- `shared/api-contracts`: DTO/API contracts dùng khi thật sự cần chia sẻ.
