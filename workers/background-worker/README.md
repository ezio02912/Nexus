# Background Worker

Worker host cho background jobs và RabbitMQ consumers.

## Trách nhiệm

- Consume integration events.
- Retry job.
- Scheduled jobs.
- Outbox/inbox dispatcher khi persistence được triển khai.

## Nguyên tắc

- Job phải idempotent.
- Log đầy đủ `TenantId`, `CorrelationId`, `EventId`.
- Không chứa business rule thuộc riêng service; worker gọi application handler của service/boundary phù hợp.
