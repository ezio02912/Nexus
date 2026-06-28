ALTER TABLE attendance_records ADD COLUMN IF NOT EXISTS department_id uuid NULL;
ALTER TABLE leave_requests ADD COLUMN IF NOT EXISTS department_id uuid NULL;

CREATE INDEX IF NOT EXISTS ix_attendance_records_employee_date ON attendance_records(tenant_id, employee_id, work_date);
CREATE INDEX IF NOT EXISTS ix_attendance_records_department_date ON attendance_records(tenant_id, department_id, work_date);
CREATE INDEX IF NOT EXISTS ix_leave_requests_employee ON leave_requests(tenant_id, employee_id);
CREATE INDEX IF NOT EXISTS ix_holidays_tenant_year ON holidays(tenant_id, year);
