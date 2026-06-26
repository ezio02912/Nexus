#!/usr/bin/env zsh
set -euo pipefail

export DOTNET_CLI_HOME="${DOTNET_CLI_HOME:-/private/tmp/dotnet-home-nexus-core}"
export NUGET_PACKAGES="${NUGET_PACKAGES:-/private/tmp/nuget-packages-nexus-core}"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE=1
export DOTNET_NOLOGO=1

mkdir -p "$DOTNET_CLI_HOME" "$NUGET_PACKAGES"

# The core platform now depends on EF Core, JWT, YARP and RabbitMQ packages which are NOT in the
# offline cache. The first build must restore online via NuGet.config; subsequent builds can run
# offline with --no-restore.
ONLINE_CONFIG="NuGet.config"

projects=(
  "shared/common-kernel/src/Nexus.SharedKernel/Nexus.SharedKernel.csproj"
  "shared/api-contracts/src/Nexus.ApiContracts/Nexus.ApiContracts.csproj"
  "shared/event-contracts/src/Nexus.EventContracts/Nexus.EventContracts.csproj"
  "shared/building-blocks/src/Nexus.BuildingBlocks/Nexus.BuildingBlocks.csproj"
  "shared/building-blocks/src/Nexus.BuildingBlocks.EntityFrameworkCore/Nexus.BuildingBlocks.EntityFrameworkCore.csproj"
  "shared/building-blocks/src/Nexus.BuildingBlocks.Web/Nexus.BuildingBlocks.Web.csproj"
  "shared/building-blocks/src/Nexus.BuildingBlocks.Messaging/Nexus.BuildingBlocks.Messaging.csproj"
  "gateways/api-gateway/src/Nexus.Gateway.Api/Nexus.Gateway.Api.csproj"
  "services/identity-service/src/Nexus.Services.Identity.Api/Nexus.Services.Identity.Api.csproj"
  "services/tenant-service/src/Nexus.Services.Tenant.Api/Nexus.Services.Tenant.Api.csproj"
  "services/permission-service/src/Nexus.Services.Permission.Api/Nexus.Services.Permission.Api.csproj"
  "services/audit-service/src/Nexus.Services.Audit.Api/Nexus.Services.Audit.Api.csproj"
  "services/file-service/src/Nexus.Services.File.Api/Nexus.Services.File.Api.csproj"
  "services/notification-service/src/Nexus.Services.Notification.Api/Nexus.Services.Notification.Api.csproj"
  "services/numbering-service/src/Nexus.Services.Numbering.Api/Nexus.Services.Numbering.Api.csproj"
  "services/workflow-service/src/Nexus.Services.Workflow.Api/Nexus.Services.Workflow.Api.csproj"
  "services/crm-service/src/Nexus.Services.Crm.Api/Nexus.Services.Crm.Api.csproj"
  "services/sales-service/src/Nexus.Services.Sales.Api/Nexus.Services.Sales.Api.csproj"
  "workers/background-worker/src/Nexus.Worker.Host/Nexus.Worker.Host.csproj"
)

dotnet build-server shutdown >/dev/null || true

for project in "${projects[@]}"; do
  echo "==> Restoring $project (online)"
  dotnet restore "$project" --configfile "$ONLINE_CONFIG" /nr:false

  echo "==> Building $project"
  dotnet build "$project" --no-restore /p:UseSharedCompilation=false /nr:false
done

echo "Core build complete."
