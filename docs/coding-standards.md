# Coding Standards

## Backend

- Domain không phụ thuộc Infrastructure.
- Application chứa use case, validation, interface ports.
- Infrastructure chứa database, broker, external clients.
- API chỉ orchestration nhẹ, mapping request/response và auth policy.
- Không dùng shared library để chia sẻ domain model giữa service.

## Clean Code

- Method ngắn, tên rõ nghĩa.
- Không nhét business rule vào controller/endpoint.
- Tách command/query khi use case lớn.
- Log có `TenantId`, `CorrelationId`, `UserId`.
- Không swallow exception.

## Testing

- Unit test cho domain rules.
- Integration test cho repository, API, event handler.
- Contract test cho event/API quan trọng.
