#!/usr/bin/env zsh
set -euo pipefail

export DOTNET_CLI_HOME="${DOTNET_CLI_HOME:-/private/tmp/dotnet-home-nexus-run}"
export NUGET_PACKAGES="${NUGET_PACKAGES:-/private/tmp/nuget-packages-nexus-run}"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE=1
export DOTNET_NOLOGO=1

mkdir -p "$DOTNET_CLI_HOME" "$NUGET_PACKAGES" logs

run_service() {
  local name="$1"
  local project="$2"
  echo "==> Starting $name"
  dotnet run --no-build --project "$project" --launch-profile http > "logs/${name}.log" 2>&1 &
}

run_service "tenant-service" "services/tenant-service/src/Nexus.Services.Tenant.Api/Nexus.Services.Tenant.Api.csproj"
run_service "identity-service" "services/identity-service/src/Nexus.Services.Identity.Api/Nexus.Services.Identity.Api.csproj"
run_service "permission-service" "services/permission-service/src/Nexus.Services.Permission.Api/Nexus.Services.Permission.Api.csproj"
run_service "audit-service" "services/audit-service/src/Nexus.Services.Audit.Api/Nexus.Services.Audit.Api.csproj"
run_service "file-service" "services/file-service/src/Nexus.Services.File.Api/Nexus.Services.File.Api.csproj"
run_service "numbering-service" "services/numbering-service/src/Nexus.Services.Numbering.Api/Nexus.Services.Numbering.Api.csproj"
run_service "workflow-service" "services/workflow-service/src/Nexus.Services.Workflow.Api/Nexus.Services.Workflow.Api.csproj"
run_service "crm-service" "services/crm-service/src/Nexus.Services.Crm.Api/Nexus.Services.Crm.Api.csproj"
run_service "sales-service" "services/sales-service/src/Nexus.Services.Sales.Api/Nexus.Services.Sales.Api.csproj"
run_service "inventory-service" "services/inventory-service/src/Nexus.Services.Inventory.Api/Nexus.Services.Inventory.Api.csproj"
run_service "purchase-service" "services/purchase-service/src/Nexus.Services.Purchase.Api/Nexus.Services.Purchase.Api.csproj"
run_service "hrm-service" "services/hrm-service/src/Nexus.Services.Hrm.Api/Nexus.Services.Hrm.Api.csproj"
run_service "attendance-service" "services/attendance-service/src/Nexus.Services.Attendance.Api/Nexus.Services.Attendance.Api.csproj"
run_service "payroll-service" "services/payroll-service/src/Nexus.Services.Payroll.Api/Nexus.Services.Payroll.Api.csproj"
run_service "notification-service" "services/notification-service/src/Nexus.Services.Notification.Api/Nexus.Services.Notification.Api.csproj"
run_service "masterdata-service" "services/masterdata-service/src/Nexus.Services.MasterData.Api/Nexus.Services.MasterData.Api.csproj"
run_service "api-gateway" "gateways/api-gateway/src/Nexus.Gateway.Api/Nexus.Gateway.Api.csproj"
run_service "background-worker" "workers/background-worker/src/Nexus.Worker.Host/Nexus.Worker.Host.csproj"

echo "Core services are starting. Logs are in ./logs."
echo ""
echo "Observability (Docker stack must be running):"
echo "  Grafana     http://localhost:3000  (admin / nexus)"
echo "  Prometheus  http://localhost:9090"
echo "  Web Admin   http://localhost:7100/monitoring"
echo ""
echo "Endpoints:"
echo "  API Gateway   http://localhost:7200"
echo "  Tenant        http://localhost:7201"
echo "  Identity      http://localhost:7202"
echo "  Permission    http://localhost:7203"
echo "  Audit         http://localhost:7204"
echo "  File          http://localhost:7205"
echo "  Numbering     http://localhost:7206"
echo "  Workflow      http://localhost:7207"
echo "  CRM           http://localhost:7208"
echo "  Sales         http://localhost:7209"
echo "  Inventory     http://localhost:7210"
echo "  Master Data   http://localhost:7211"
echo "  Purchase      http://localhost:7212"
echo "  Notification  http://localhost:7213"
echo "  HRM           http://localhost:7214"
echo "  Attendance    http://localhost:7215"
echo "  Payroll       http://localhost:7216"
