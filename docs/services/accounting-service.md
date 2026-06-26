# Accounting Service

## Trách nhiệm

- Hệ thống tài khoản.
- Sổ cái, bút toán, phiếu thu/chi.
- Công nợ phải thu/phải trả.
- Báo cáo tài chính và báo cáo thuế ở phase sau.

## Data dự kiến

- ChartOfAccounts
- JournalEntries
- JournalEntryLines
- Receivables
- Payables
- Payments
- BankTransactions
- AccountingPeriods

## Rule quan trọng

Accounting không join trực tiếp database invoice/sales/payroll. Service nhận event và lưu snapshot:

- `SourceModule`
- `SourceType`
- `SourceId`
- `DocumentNo`
- `Amount`
- `PostingDate`

## Events

- Consume `InvoiceIssued`, `InvoicePaid`, `PayrollApproved`, `GoodsReceived`.
- Publish `JournalEntryPosted`.
