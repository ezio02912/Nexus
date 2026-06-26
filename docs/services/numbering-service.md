# Numbering Service

## Trách nhiệm

- Sinh số chứng từ tự động theo tenant/module/năm/kỳ.
- Đảm bảo không trùng số trong cùng sequence.
- Hỗ trợ preview và reserve number.

## Data dự kiến

- NumberSequences
- NumberSequenceRules
- NumberReservations

## API dự kiến

- `POST /numbering/next`
- `POST /numbering/reserve`
- `GET /numbering/sequences`

## Events

- `DocumentNumberReserved`
- `DocumentNumberIssued`
