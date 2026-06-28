# Local Development

## Infrastructure

```bash
cp .env.example .env
docker compose -f deploy/docker-compose/docker-compose.yml up -d
```

## .NET build

```bash
DOTNET_CLI_HOME=/private/tmp/dotnet-home dotnet build Nexus.SaasPlatform.slnx
```

Web tenant only:

```bash
tools/scripts/build-web-tenant.zsh
```

## Migrations

```bash
tools/scripts/apply-core-migrations.zsh
```

CRM/Sales/Inventory/Purchase/HRM/Attendance/Payroll SQL migrations use `psql` and default to:

- CRM: `crm_db`
- Sales: `sales_db`
- Inventory: `inventory_db`
- Purchase: `purchase_db`
- HRM: `hrm_db`
- Attendance: `attendance_db`
- Payroll: `payroll_db`

After migration and service startup, web tenant demo login is:

- Tenant code: `DEMO`
- User name: `tenantadmin`
- Password: `123123`

## Service port convention

- Gateway: `7200`
- Web Admin: `7100`
- Web Tenant: `7101`
- Tenant: `7201`
- Identity: `7202`
- Permission: `7203`
- Audit: `7204`
- File: `7205`
- Numbering: `7206`
- Workflow: `7207`
- CRM: `7208`
- Sales: `7209`
- Inventory: `7210`
- Master Data: `7211`
- Purchase: `7212`
- Notification: `7213`
- HRM: `7214`
- Attendance: `7215`
- Payroll: `7216`
- Workers: no public port by default

## Observability

```bash
docker compose -f deploy/docker-compose/docker-compose.yml up -d
./tools/scripts/run-core-services.zsh
./tools/scripts/open-observability.zsh
```

- Grafana: http://localhost:3000 (admin / nexus)
- Web Admin monitoring: http://localhost:7100/monitoring

Chi tiết: `docs/observability.md`.
