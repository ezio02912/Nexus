# Report Service

## Trách nhiệm

- Read models và báo cáo tổng hợp.
- Không sở hữu business transaction gốc.
- Nhận event để cập nhật report projection.

## Data dự kiến

- ReportDefinitions
- ReportSnapshots
- ProjectionStates

## Reports dự kiến

- Công nợ.
- Sổ cái.
- Doanh thu.
- Tồn kho.
- Lương.

## Events

- Consume events từ CRM, Sales, Inventory, Invoice, Accounting, Payroll.
