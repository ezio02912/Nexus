#!/usr/bin/env zsh
set -euo pipefail

# Applies EF Core migrations for every core service database.
# Requires the dotnet-ef local tool (restored automatically below) and a running PostgreSQL.

export DOTNET_CLI_HOME="${DOTNET_CLI_HOME:-/private/tmp/dotnet-home-nexus-core}"
export NUGET_PACKAGES="${NUGET_PACKAGES:-/private/tmp/nuget-packages-nexus-core}"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

mkdir -p "$DOTNET_CLI_HOME" "$NUGET_PACKAGES"

echo "==> Restoring local tools (dotnet-ef)"
dotnet tool restore

apply() {
  local name="$1"
  local project="$2"
  local startup="$3"
  echo "==> Updating database for $name"
  dotnet ef database update --project "$project" --startup-project "$startup"
}

apply_sql_folder() {
  local name="$1"
  local database="$2"
  local folder="$3"

  if ! command -v psql >/dev/null 2>&1; then
    echo "==> Skipping $name SQL migrations because psql is not installed."
    return
  fi

  echo "==> Applying SQL migrations for $name"
  for file in "$folder"/*.sql; do
    [ -e "$file" ] || continue
    echo "    $file"
    PGPASSWORD="${POSTGRES_PASSWORD:-nexus_dev_password}" psql \
      -h "${POSTGRES_HOST:-localhost}" \
      -p "${POSTGRES_PORT:-5432}" \
      -U "${POSTGRES_USER:-nexus}" \
      -d "$database" \
      -v ON_ERROR_STOP=1 \
      -f "$file"
  done
}

apply "identity"     "services/identity-service/src/Nexus.Services.Identity.Infrastructure" "services/identity-service/src/Nexus.Services.Identity.Api"
apply "tenant"       "services/tenant-service/src/Nexus.Services.Tenant.Infrastructure"     "services/tenant-service/src/Nexus.Services.Tenant.Api"
apply "permission"   "services/permission-service/src/Nexus.Services.Permission.Api"        "services/permission-service/src/Nexus.Services.Permission.Api"
apply "audit"        "services/audit-service/src/Nexus.Services.Audit.Api"                  "services/audit-service/src/Nexus.Services.Audit.Api"
apply "file"         "services/file-service/src/Nexus.Services.File.Api"                    "services/file-service/src/Nexus.Services.File.Api"
apply "notification" "services/notification-service/src/Nexus.Services.Notification.Api"    "services/notification-service/src/Nexus.Services.Notification.Api"
apply "workflow"     "services/workflow-service/src/Nexus.Services.Workflow.Api"            "services/workflow-service/src/Nexus.Services.Workflow.Api"
apply "numbering"    "services/numbering-service/src/Nexus.Services.Numbering.Api"          "services/numbering-service/src/Nexus.Services.Numbering.Api"
apply_sql_folder "crm"   "${CRM_DB:-crm_db}"     "services/crm-service/migrations"
apply_sql_folder "sales" "${SALES_DB:-sales_db}" "services/sales-service/migrations"
apply_sql_folder "inventory" "${INVENTORY_DB:-inventory_db}" "services/inventory-service/migrations"
apply_sql_folder "purchase"  "${PURCHASE_DB:-purchase_db}"   "services/purchase-service/migrations"

echo "All core migrations applied."
