# Payroll Service

## Trách nhiệm

- Kỳ lương.
- Bảng lương, phụ cấp, khấu trừ.
- Thuế TNCN, bảo hiểm.
- Duyệt và thanh toán lương.

## Data dự kiến

- PayrollPeriods
- PayrollRuns
- PayrollLines
- Allowances
- Deductions
- PayrollPayments

## Events

- `PayrollCalculated`
- `PayrollApproved`
- `PayrollPaid`

## Tích hợp kế toán

Sau khi `PayrollApproved`, Accounting Service sinh bút toán chi phí lương dựa trên snapshot từ event.
