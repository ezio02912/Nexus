# API Gateway

Gateway/BFF cho web và mobile.

## Phase hiện tại

- Project .NET 10 tối giản.
- Chuẩn bị vị trí cho YARP/Ocelot ở phase sau.
- Health endpoint dùng để kiểm tra local environment.

## Trách nhiệm

- Route request đến service phù hợp.
- Auth/token validation.
- Tenant context propagation.
- Correlation id propagation.
- Rate limit theo tenant/package.
