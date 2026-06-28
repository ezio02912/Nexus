# Attendance Service

## Trách nhiệm

- Lịch làm việc theo quốc gia/tenant, mặc định Việt Nam T2-T6 08:00-17:00, nghỉ trưa 12:00-13:00.
- Ngày nghỉ lễ quản bằng master data theo năm.
- Ca làm, phân ca, check-in/check-out, đi muộn, về sớm, sửa công cần duyệt.
- Quản lý loại phép, số dư phép, đơn phép và tăng ca.
- Phép năm mặc định 12 ngày/năm, có chỗ cấu hình carry-forward và quota theo policy.

## Data

- WorkCalendars
- Holidays
- Shifts
- ShiftAssignments
- AttendanceRecords
- LeaveTypes
- LeaveBalances
- LeaveRequests
- OvertimeRequests

## API chính

- `GET/POST /api/attendance/work-calendars`
- `GET/POST /api/attendance/holidays`
- `GET/POST /api/attendance/shifts`
- `GET/POST /api/attendance/shift-assignments`
- `GET/POST /api/attendance/records`
- `GET/POST /api/attendance/leave-types`
- `GET/POST /api/attendance/leave-balances`
- `GET/POST /api/attendance/leave-requests`
- `GET/POST /api/attendance/overtime-requests`
- `POST /api/attendance/setup-vn-defaults`
- `POST /api/attendance/leave-requests/{id}/approve`
- `POST /api/attendance/leave-requests/{id}/reject`
- `POST /api/attendance/overtime-requests/{id}/approve`

## Events

- `AttendanceRecorded`
- `LeaveApproved`
- `OvertimeApproved`
