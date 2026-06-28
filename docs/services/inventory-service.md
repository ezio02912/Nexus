# Inventory Service

## Trách nhiệm

- Sản phẩm, kho, nhập/xuất/chuyển kho.
- Tồn kho và kiểm kê.

## Data hiện tại

- Products
- Warehouses
- StockMovements
- StockBalances
- StockReservations

## Data dự kiến

- StockTransfers
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
- Reservation line nhận `WarehouseCode`, `ProductCode`, mô tả và số lượng để giữ/xuất tồn theo đúng kho trên từng dòng Sales Order.
- Request cũ không truyền `WarehouseCode` vẫn fallback về kho `MAIN`.
- Nếu warehouse bật `AllowNegativeStock`, reservation được phép giữ vượt tồn khả dụng để xử lý đơn chờ hàng về.

## Purchase integration

- Purchase Order receive tạo Goods Receipt trong Purchase Service.
- Purchase Service gọi Inventory stock import với source `PURCHASE_RECEIPT`.
- Inventory tạo movement `IN` và tăng `OnHandQuantity` theo warehouse/product của từng dòng receipt.

## Tenant Web

- `/inventory` là page riêng cho tồn kho và nhập kho nhanh.
- `/inventory/products` là page riêng cho mã hàng hoá/catalog.
- `/inventory/warehouses` là page riêng cho danh mục kho.
- Tồn kho hiển thị On hand, Reserved, Available.
- Nhập kho nhanh tự tạo product/warehouse catalog nếu chưa có.
- Product catalog có SKU, đơn vị tính lookup từ Master Data `Unit`, loại hàng hoá lookup từ Master Data `ProductType`, ảnh/tệp qua File Service, thuộc tính, biến thể, giá, thuế, active state.
- Warehouse catalog có mã kho, tên kho, vị trí, active state và cấu hình cho phép tồn kho âm.
- Stock balances phản ánh cả nhập kho thủ công và nhập kho từ Purchase Goods Receipt.

## Migration

- SQL migrations mới:
  - `services/inventory-service/migrations/003_add_product_catalog_metadata.sql`
  - `services/inventory-service/migrations/006_add_warehouse_negative_stock.sql`
- Chạy thủ công bằng `./tools/scripts/apply-core-migrations.zsh` hoặc apply riêng folder `services/inventory-service/migrations` vào `inventory_db`.

## Events

- `StockImported`
- `StockExported`
- `InventoryAdjusted`
