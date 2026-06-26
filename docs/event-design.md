# Event Design

## Envelope chuẩn

```json
{
  "eventId": "uuid",
  "eventName": "InvoiceIssued",
  "tenantId": "tenant-id",
  "occurredAt": "2026-06-26T10:00:00Z",
  "sourceService": "invoice-service",
  "correlationId": "uuid",
  "data": {}
}
```

## Nguyên tắc

- Event là integration contract, không expose entity nội bộ.
- Consumer phải idempotent theo `eventId`.
- Event quan trọng cần outbox/inbox khi bắt đầu triển khai persistence.
- Accounting nhận event và lưu snapshot chứng từ cần thiết.

## Event nhóm đầu

- `TenantCreated`
- `UserCreated`
- `FileUploaded`
- `AuditLogRecorded`
- `WorkflowApproved`
- `CustomerCreated`
- `QuotationApproved`
- `SalesOrderCreated`
- `GoodsReceived`
- `InvoiceIssued`
- `PayrollApproved`
