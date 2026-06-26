# Observability & Monitoring

Nexus dùng **Grafana** làm trung tâm giám sát: xem log tập trung (Loki), metrics (Prometheus) và trạng thái dịch vụ.

## Khởi động stack

```bash
cp .env.example .env
docker compose -f deploy/docker-compose/docker-compose.yml up -d
```

Sau khi stack chạy:

| Công cụ | URL | Ghi chú |
|---------|-----|---------|
| **Grafana** | http://localhost:3000 | User `admin` / pass `nexus` (dev) |
| **Prometheus** | http://localhost:9090 | Scrape `/metrics` từ các service |
| **Loki** | http://localhost:3100 | API log (dùng qua Grafana) |
| **RabbitMQ** | http://localhost:15672 | User/pass từ `.env` |
| **Mailpit** | http://localhost:8025 | Email dev |
| **MinIO** | http://localhost:9001 | Object storage console |

## Dashboard có sẵn trong Grafana

- **Nexus Platform Logs** — log tất cả microservice (Loki)
- **Nexus Platform Overview** — availability, request rate, latency (Prometheus)

Folder Grafana: `Nexus Platform`.

## Thu thập log

1. Chạy core services (log ghi vào `./logs/*.log`):

```bash
./tools/scripts/run-core-services.zsh
```

2. **Promtail** đọc thư mục `logs/` và đẩy lên Loki.
3. Mỗi service dùng **Serilog JSON** (compact) với property `Service` để lọc theo tên dịch vụ.

Truy vấn LogQL mẫu trong Grafana Explore:

```logql
{job="nexus-services", service="tenant-service"}
{job="nexus-services"} |= "Error"
```

## Metrics

Các API service expose endpoint Prometheus:

```text
GET http://localhost:7201/metrics
```

Prometheus trong Docker scrape qua `host.docker.internal` (port 7200–7211).

## Web Admin — trang Giám sát

Trong **Web Admin** → menu **Giám sát** → **Giám sát hệ thống** (`/monitoring`):

- Bảng health check từng service
- Link nhanh tới Grafana, Prometheus, RabbitMQ, Mailpit, MinIO
- Iframe Grafana Explore để xem log trực tiếp

## Cấu trúc file

```text
deploy/docker-compose/observability/
  loki-config.yml
  promtail-config.yml
  prometheus.yml
  grafana/provisioning/...
  grafana/dashboards/...
shared/building-blocks/src/Nexus.BuildingBlocks.Observability/
```

## Tích hợp vào service mới

```csharp
using Nexus.BuildingBlocks.Observability;

var builder = WebApplication.CreateBuilder(args);
builder.AddNexusObservability("my-service");

// ... register services ...

var app = builder.Build();
app.MapNexusObservability();
app.Run();
```

Worker (không có HTTP):

```csharp
builder.AddNexusWorkerObservability("my-worker");
```

Thêm port vào `deploy/docker-compose/observability/prometheus.yml` và `MonitoringService` nếu cần hiển thị trên Web Admin.
