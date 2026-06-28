CREATE TABLE IF NOT EXISTS work_calendars (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    calendar_code varchar(64) NOT NULL,
    name varchar(256) NOT NULL,
    country_code varchar(16) NOT NULL DEFAULT 'VN',
    working_days varchar(128) NOT NULL DEFAULT 'Mon,Tue,Wed,Thu,Fri',
    default_start_time time NOT NULL DEFAULT '08:00',
    default_end_time time NOT NULL DEFAULT '17:00',
    break_start_time time NOT NULL DEFAULT '12:00',
    break_end_time time NOT NULL DEFAULT '13:00',
    standard_hours_per_day numeric(5,2) NOT NULL DEFAULT 8,
    standard_hours_per_week numeric(5,2) NOT NULL DEFAULT 40,
    grace_late_minutes integer NOT NULL DEFAULT 5,
    grace_early_minutes integer NOT NULL DEFAULT 5,
    is_default boolean NOT NULL DEFAULT false,
    status varchar(64) NOT NULL DEFAULT 'Active',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS holidays (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    holiday_date date NOT NULL,
    name varchar(256) NOT NULL,
    holiday_type varchar(64) NOT NULL DEFAULT 'Public',
    is_paid boolean NOT NULL DEFAULT true,
    country_code varchar(16) NOT NULL DEFAULT 'VN',
    year integer NOT NULL,
    source varchar(128) NOT NULL DEFAULT 'MasterData',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS shifts (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    shift_code varchar(64) NOT NULL,
    name varchar(256) NOT NULL,
    start_time time NOT NULL,
    end_time time NOT NULL,
    break_start_time time NOT NULL,
    break_end_time time NOT NULL,
    cross_day boolean NOT NULL DEFAULT false,
    standard_hours numeric(5,2) NOT NULL DEFAULT 8,
    late_grace_minutes integer NOT NULL DEFAULT 5,
    early_leave_grace_minutes integer NOT NULL DEFAULT 5,
    status varchar(64) NOT NULL DEFAULT 'Active',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS shift_assignments (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    employee_id uuid NOT NULL,
    shift_id uuid NOT NULL,
    work_date date NOT NULL,
    assignment_type varchar(64) NOT NULL DEFAULT 'Manual',
    department_id uuid NULL,
    approved_by uuid NULL,
    approved_at timestamptz NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS attendance_records (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    employee_id uuid NOT NULL,
    work_date date NOT NULL,
    shift_id uuid NULL,
    check_in_at timestamptz NULL,
    check_out_at timestamptz NULL,
    check_in_source varchar(64) NOT NULL DEFAULT '',
    check_out_source varchar(64) NOT NULL DEFAULT '',
    check_in_latitude numeric(10,6) NULL,
    check_in_longitude numeric(10,6) NULL,
    check_out_latitude numeric(10,6) NULL,
    check_out_longitude numeric(10,6) NULL,
    late_minutes integer NOT NULL DEFAULT 0,
    early_leave_minutes integer NOT NULL DEFAULT 0,
    worked_minutes integer NOT NULL DEFAULT 0,
    overtime_minutes integer NOT NULL DEFAULT 0,
    status varchar(64) NOT NULL DEFAULT 'Draft',
    correction_status varchar(64) NOT NULL DEFAULT 'None',
    correction_reason text NOT NULL DEFAULT '',
    approved_by uuid NULL,
    approved_at timestamptz NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS leave_types (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    leave_type_code varchar(64) NOT NULL,
    name varchar(256) NOT NULL,
    is_paid boolean NOT NULL DEFAULT true,
    annual_quota_days numeric(5,2) NOT NULL DEFAULT 12,
    carry_forward_allowed boolean NOT NULL DEFAULT false,
    max_carry_forward_days numeric(5,2) NOT NULL DEFAULT 0,
    requires_approval boolean NOT NULL DEFAULT true,
    status varchar(64) NOT NULL DEFAULT 'Active',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS leave_balances (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    employee_id uuid NOT NULL,
    year integer NOT NULL,
    leave_type_id uuid NOT NULL,
    opening_days numeric(6,2) NOT NULL DEFAULT 0,
    accrued_days numeric(6,2) NOT NULL DEFAULT 0,
    used_days numeric(6,2) NOT NULL DEFAULT 0,
    pending_days numeric(6,2) NOT NULL DEFAULT 0,
    adjusted_days numeric(6,2) NOT NULL DEFAULT 0,
    remaining_days numeric(6,2) NOT NULL DEFAULT 0,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS leave_requests (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    request_no varchar(64) NOT NULL,
    employee_id uuid NOT NULL,
    leave_type_id uuid NOT NULL,
    from_date date NOT NULL,
    to_date date NOT NULL,
    total_days numeric(6,2) NOT NULL DEFAULT 0,
    reason text NOT NULL DEFAULT '',
    status varchar(64) NOT NULL DEFAULT 'Pending',
    approver_id uuid NULL,
    approved_at timestamptz NULL,
    rejected_reason text NOT NULL DEFAULT '',
    attachment_file_id uuid NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS overtime_requests (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    request_no varchar(64) NOT NULL,
    employee_id uuid NOT NULL,
    work_date date NOT NULL,
    from_time time NOT NULL,
    to_time time NOT NULL,
    total_hours numeric(6,2) NOT NULL DEFAULT 0,
    overtime_type varchar(64) NOT NULL DEFAULT 'Weekday',
    rate_multiplier numeric(5,2) NOT NULL DEFAULT 1.5,
    reason text NOT NULL DEFAULT '',
    status varchar(64) NOT NULL DEFAULT 'Pending',
    approver_id uuid NULL,
    approved_at timestamptz NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_work_calendars_tenant_code ON work_calendars(tenant_id, calendar_code);
CREATE UNIQUE INDEX IF NOT EXISTS ux_shifts_tenant_code ON shifts(tenant_id, shift_code);
CREATE UNIQUE INDEX IF NOT EXISTS ux_leave_types_tenant_code ON leave_types(tenant_id, leave_type_code);
CREATE UNIQUE INDEX IF NOT EXISTS ux_leave_balances_tenant_employee_year_type ON leave_balances(tenant_id, employee_id, year, leave_type_id);
CREATE UNIQUE INDEX IF NOT EXISTS ux_leave_requests_tenant_no ON leave_requests(tenant_id, request_no);
CREATE UNIQUE INDEX IF NOT EXISTS ux_overtime_requests_tenant_no ON overtime_requests(tenant_id, request_no);
