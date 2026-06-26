# Common Kernel

Shared primitives dùng chung nhưng không chứa domain model của từng service.

Ví dụ phù hợp:

- `TenantId`
- `CorrelationId`
- Base entity primitive.
- Clock abstraction.

Không đặt `Customer`, `Invoice`, `Employee` ở đây.
