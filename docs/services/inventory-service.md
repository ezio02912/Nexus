# Inventory Service

## Trách nhiệm

- Sản phẩm, kho, nhập/xuất/chuyển kho.
- Tồn kho và kiểm kê.

## Data dự kiến

- Products
- Warehouses
- StockMovements
- StockBalances
- InventoryAdjustments

## API hiện tại

- `GET /api/inventory/balances?tenantId={tenantId}&search={search}`
- `GET /api/inventory/products?tenantId={tenantId}&search={search}`
- `POST /api/inventory/products`
- `GET /api/inventory/warehouses?tenantId={tenantId}&search={search}`
- `POST /api/inventory/warehouses`
- `POST /api/inventory/stock/import`
- `POST /api/inventory/reservations`
- `POST /api/inventory/shipments`

## Sales integration

- Sales Order approve gọi Inventory reservation theo source `SALES_ORDER`.
- Sales Order deliver gọi Inventory shipment, trừ `ReservedQuantity` và `OnHandQuantity`.
- Reservation/shipment idempotent theo `TenantId + SourceType + SourceId`.
- Kho mặc định cho Sales reservation hiện là `MAIN`; Inventory UI đã có danh mục kho để chuẩn bị chọn kho trên từng dòng hàng.

## Tenant Web

- `/inventory` có tab Tồn kho, Sản phẩm và Kho.
- Tồn kho hiển thị On hand, Reserved, Available.
- Nhập kho nhanh tự tạo product/warehouse catalog nếu chưa có.
- Product catalog có SKU, đơn vị, nhóm hàng, giá, thuế, active state.
- Warehouse catalog có mã kho, tên kho, vị trí, active state.

## Events

- `StockImported`
- `StockExported`
- `InventoryAdjusted`
