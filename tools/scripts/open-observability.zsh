#!/usr/bin/env zsh
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"

echo "==> Opening Nexus observability URLs"
echo ""
echo "Grafana (logs + dashboards):  http://localhost:3000  (admin / nexus)"
echo "Prometheus:                     http://localhost:9090"
echo "Web Admin monitoring page:      http://localhost:7100/monitoring"
echo "RabbitMQ management:            http://localhost:15672"
echo ""
echo "Ensure infrastructure is running:"
echo "  docker compose -f deploy/docker-compose/docker-compose.yml up -d"
echo "Ensure services are running:"
echo "  ./tools/scripts/run-core-services.zsh"
