#!/usr/bin/env zsh
set -euo pipefail

export DOTNET_CLI_HOME="${DOTNET_CLI_HOME:-/private/tmp/dotnet-home-nexus-web}"
export NUGET_PACKAGES="${NUGET_PACKAGES:-/private/tmp/nuget-packages-nexus-web}"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE=1
export DOTNET_NOLOGO=1

mkdir -p "$DOTNET_CLI_HOME" "$NUGET_PACKAGES"

project="apps/web-admin/src/Nexus.Web.Admin/Nexus.Web.Admin.csproj"

dotnet build-server shutdown >/dev/null || true

set +e
dotnet restore "$project" \
  --disable-parallel \
  /p:RestoreUseStaticGraphEvaluation=true \
  /nr:false
restore_status=$?
set -e

assets_file="${project:h}/obj/project.assets.json"
if [[ $restore_status -ne 0 && ! -f "$assets_file" ]]; then
  echo "Restore failed and $assets_file was not created."
  exit $restore_status
fi

dotnet build "$project" \
  --no-restore \
  /p:UseSharedCompilation=false \
  /nr:false
