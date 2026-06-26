# Nexus SaaS Platform

Base source cho hệ thống SaaS Enterprise Platform theo hướng microservices, .NET 10, Blazor, PostgreSQL, Redis, RabbitMQ và worker nền.

## Mục tiêu hiện tại

- Dựng monorepo và structure folder.
- Chuẩn bị backend .NET 10 theo Clean Architecture.
- Chuẩn bị frontend Blazor và source BootstrapBlazor ở nhánh/vendor riêng.
- Chuẩn bị folder mobile Flutter, chưa triển khai app.
- Tạo tài liệu kiến trúc và tài liệu cho từng service.

## Cấu trúc chính

- `apps/web-admin`: Blazor admin portal.
- `apps/web-tenant`: Blazor tenant portal.
- `apps/mobile`: Flutter mobile placeholder.
- `gateways/api-gateway`: API Gateway/BFF.
- `services`: Các microservice nghiệp vụ.
- `shared`: Shared kernel, building blocks, contracts.
- `workers/background-worker`: Worker host cho background jobs và consumers.
- `deploy`: Docker Compose và Kubernetes manifests.
- `external/bootstrap-blazor`: Source BootstrapBlazor, branch `vendor/bootstrapblazor-latest`.
- `docs`: Tài liệu kiến trúc, vận hành và service docs.

## Chạy local infrastructure

```bash
cp .env.example .env
docker compose -f deploy/docker-compose/docker-compose.yml up -d
./tools/scripts/apply-core-migrations.zsh
```

## Build source .NET

```bash
DOTNET_CLI_HOME=/private/tmp/dotnet-home dotnet build Nexus.SaasPlatform.slnx
```

Core services có script zsh riêng:

```bash
./tools/scripts/build-core.zsh
```

Build web admin:

```bash
./tools/scripts/build-web-admin.zsh
```

Run local core services:

```bash
./tools/scripts/run-core-services.zsh
dotnet run --project apps/web-admin/src/Nexus.Web.Admin/Nexus.Web.Admin.csproj --launch-profile http
```

Web Admin login flow xem tại `docs/web-admin.md`.

Build Tenant + Identity riêng:

```bash
./tools/scripts/build-tenant-identity.zsh
```

## Nguyên tắc

- Mỗi service sở hữu database riêng.
- Không join database trực tiếp giữa các service.
- Giao tiếp đồng bộ qua REST/gateway, giao tiếp bất đồng bộ qua RabbitMQ event.
- Mọi dữ liệu nghiệp vụ cần có `TenantId`.
- Audit, workflow, file attachment và source document là năng lực nền tảng.
# Nexus
# Nexus



tools/scripts/run-core-services.zsh

dotnet run --project apps/web-admin/src/Nexus.Web.Admin/Nexus.Web.Admin.csproj --launch-profile http

dotnet run --project apps/web-tenant/src/Nexus.Web.Tenant/Nexus.Web.Tenant.csproj --launch-profile http# Nexus
