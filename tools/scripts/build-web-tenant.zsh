#!/usr/bin/env zsh
set -euo pipefail

export DOTNET_CLI_HOME="${DOTNET_CLI_HOME:-/private/tmp/dotnet-home-nexus-web-tenant}"
export NUGET_PACKAGES="${NUGET_PACKAGES:-/private/tmp/nuget-packages-nexus-web-tenant}"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE=1
export DOTNET_NOLOGO=1

mkdir -p "$DOTNET_CLI_HOME" "$NUGET_PACKAGES"

dotnet restore "apps/web-tenant/src/Nexus.Web.Tenant/Nexus.Web.Tenant.csproj" --configfile "NuGet.config" /nr:false
dotnet build "apps/web-tenant/src/Nexus.Web.Tenant/Nexus.Web.Tenant.csproj" --no-restore /p:UseSharedCompilation=false /nr:false
