# Payroll Service

## Trách nhiệm

- Cấu hình chính sách lương theo quốc gia và ngày hiệu lực.
- Payroll Việt Nam đầy đủ: BHXH, BHYT, BHTN phần người lao động/người sử dụng lao động, công đoàn, giảm trừ cá nhân/phụ thuộc, thuế TNCN lũy tiến.
- Thành phần lương: lương cơ bản, phụ cấp, khấu trừ, tăng ca, taxable, insurance included, recurring, formula.
- Kỳ lương, bảng lương, dòng lương, chi tiết thành phần, phiếu lương và chi lương.
- Không posting kế toán trực tiếp; Accounting ở Final Phase consume snapshot/event.

## Data

- PayrollPolicies
- SalaryComponents
- PayrollPeriods
- PayrollRuns
- PayrollLines
- PayrollLineComponents
- Payslips
- PayrollPayments

## API chính

- `GET/POST /api/payroll/policies`
- `GET/POST /api/payroll/components`
- `GET/POST /api/payroll/periods`
- `GET/POST /api/payroll/runs`
- `GET/POST /api/payroll/lines`
- `GET/POST /api/payroll/line-components`
- `GET/POST /api/payroll/payslips`
- `GET/POST /api/payroll/payments`
- `POST /api/payroll/setup-vn-defaults`
- `POST /api/payroll/runs/{id}/calculate`
- `POST /api/payroll/runs/{id}/approve`
- `POST /api/payroll/runs/{id}/publish-payslips`
- `POST /api/payroll/runs/{id}/pay`

## Events

- `PayrollCalculated`
- `PayrollApproved`
- `PayrollPaid`

## Tích hợp kế toán

Sau khi `PayrollApproved` hoặc `PayrollPaid`, Payroll giữ snapshot để Accounting Service ở Final Phase sinh bút toán chi phí lương, phải trả và thanh toán.
