# Audit Service

## Trách nhiệm

- Ghi audit log toàn hệ thống.
- Lưu thay đổi trước/sau của entity.
- Ghi user access log, IP, user agent, correlation id.

## Data dự kiến

- AuditLogs
- EntityChangeLogs
- UserAccessLogs

## API dự kiến

- `GET /audit-logs`
- `GET /entity-change-logs`
- `GET /user-access-logs`

## Events

- `AuditLogRecorded`
- `EntityChanged`
