# ERP Service

## Trách nhiệm

ERP Service là boundary dự phòng cho các luồng ERP tổng hợp chưa đủ rõ để tách vào Purchase, Inventory, Sales hoặc Accounting.

## Nguyên tắc

- Không biến ERP Service thành service chứa mọi thứ.
- Khi một domain đủ rõ, tách về service chuyên trách.
- Chỉ giữ orchestration hoặc master data ERP thật sự dùng chung.

## Data dự kiến

- ERPSettings
- BusinessUnits
- CostCenters

## Events

- `BusinessUnitCreated`
- `CostCenterCreated`
