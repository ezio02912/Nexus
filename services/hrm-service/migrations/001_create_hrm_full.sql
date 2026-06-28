CREATE TABLE IF NOT EXISTS employees (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    employee_code varchar(64) NOT NULL,
    full_name varchar(256) NOT NULL,
    display_name varchar(256) NOT NULL DEFAULT '',
    gender varchar(32) NOT NULL DEFAULT '',
    date_of_birth date NULL,
    nationality varchar(32) NOT NULL DEFAULT 'VN',
    marital_status varchar(32) NOT NULL DEFAULT '',
    personal_email varchar(256) NOT NULL DEFAULT '',
    work_email varchar(256) NOT NULL DEFAULT '',
    phone varchar(64) NOT NULL DEFAULT '',
    emergency_contact_name varchar(256) NOT NULL DEFAULT '',
    emergency_contact_phone varchar(64) NOT NULL DEFAULT '',
    identity_no varchar(64) NOT NULL DEFAULT '',
    identity_issued_date date NULL,
    identity_issued_place varchar(256) NOT NULL DEFAULT '',
    tax_code varchar(64) NOT NULL DEFAULT '',
    social_insurance_no varchar(64) NOT NULL DEFAULT '',
    permanent_address text NOT NULL DEFAULT '',
    current_address text NOT NULL DEFAULT '',
    bank_name varchar(256) NOT NULL DEFAULT '',
    bank_account_no varchar(64) NOT NULL DEFAULT '',
    bank_account_name varchar(256) NOT NULL DEFAULT '',
    department_id uuid NULL,
    position_id uuid NULL,
    manager_id uuid NULL,
    employment_status varchar(64) NOT NULL DEFAULT 'Draft',
    employment_type varchar(64) NOT NULL DEFAULT 'FullTime',
    join_date date NULL,
    probation_start_date date NULL,
    probation_end_date date NULL,
    official_date date NULL,
    resign_date date NULL,
    resign_reason text NOT NULL DEFAULT '',
    base_salary numeric(18,2) NOT NULL DEFAULT 0,
    salary_currency varchar(16) NOT NULL DEFAULT 'VND',
    payroll_group_id uuid NULL,
    work_calendar_id uuid NULL,
    avatar_file_id uuid NULL,
    owner_id uuid NULL,
    notes text NOT NULL DEFAULT '',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS departments (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    department_code varchar(64) NOT NULL,
    name varchar(256) NOT NULL,
    parent_department_id uuid NULL,
    manager_id uuid NULL,
    cost_center_code varchar(64) NOT NULL DEFAULT '',
    location varchar(256) NOT NULL DEFAULT '',
    status varchar(64) NOT NULL DEFAULT 'Active',
    description text NOT NULL DEFAULT '',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS positions (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    position_code varchar(64) NOT NULL,
    name varchar(256) NOT NULL,
    department_id uuid NULL,
    level varchar(64) NOT NULL DEFAULT '',
    job_grade varchar(64) NOT NULL DEFAULT '',
    min_salary numeric(18,2) NOT NULL DEFAULT 0,
    max_salary numeric(18,2) NOT NULL DEFAULT 0,
    description text NOT NULL DEFAULT '',
    status varchar(64) NOT NULL DEFAULT 'Active',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS employee_contracts (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    contract_no varchar(64) NOT NULL,
    employee_id uuid NOT NULL,
    contract_type varchar(64) NOT NULL DEFAULT '',
    status varchar(64) NOT NULL DEFAULT 'Draft',
    effective_from date NULL,
    effective_to date NULL,
    signed_date date NULL,
    signed_by uuid NULL,
    base_salary numeric(18,2) NOT NULL DEFAULT 0,
    allowance_amount numeric(18,2) NOT NULL DEFAULT 0,
    currency varchar(16) NOT NULL DEFAULT 'VND',
    working_location varchar(256) NOT NULL DEFAULT '',
    probation_terms text NOT NULL DEFAULT '',
    termination_terms text NOT NULL DEFAULT '',
    file_id uuid NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS employee_histories (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    employee_id uuid NOT NULL,
    change_type varchar(64) NOT NULL DEFAULT '',
    effective_date date NOT NULL,
    old_value text NOT NULL DEFAULT '',
    new_value text NOT NULL DEFAULT '',
    reason text NOT NULL DEFAULT '',
    approved_by uuid NULL,
    approved_at timestamptz NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS job_requisitions (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    requisition_no varchar(64) NOT NULL,
    title varchar(256) NOT NULL,
    department_id uuid NULL,
    position_id uuid NULL,
    hiring_manager_id uuid NULL,
    recruiter_id uuid NULL,
    headcount integer NOT NULL DEFAULT 0,
    employment_type varchar(64) NOT NULL DEFAULT 'FullTime',
    work_location varchar(256) NOT NULL DEFAULT '',
    salary_min numeric(18,2) NOT NULL DEFAULT 0,
    salary_max numeric(18,2) NOT NULL DEFAULT 0,
    currency varchar(16) NOT NULL DEFAULT 'VND',
    reason text NOT NULL DEFAULT '',
    priority varchar(64) NOT NULL DEFAULT 'Normal',
    status varchar(64) NOT NULL DEFAULT 'Open',
    opened_date date NULL,
    target_start_date date NULL,
    closed_date date NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS candidates (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    candidate_code varchar(64) NOT NULL,
    full_name varchar(256) NOT NULL,
    email varchar(256) NOT NULL DEFAULT '',
    phone varchar(64) NOT NULL DEFAULT '',
    source varchar(128) NOT NULL DEFAULT '',
    current_company varchar(256) NOT NULL DEFAULT '',
    current_title varchar(256) NOT NULL DEFAULT '',
    expected_salary numeric(18,2) NOT NULL DEFAULT 0,
    currency varchar(16) NOT NULL DEFAULT 'VND',
    notice_period_days integer NOT NULL DEFAULT 0,
    resume_file_id uuid NULL,
    portfolio_url text NOT NULL DEFAULT '',
    status varchar(64) NOT NULL DEFAULT 'New',
    tags text NOT NULL DEFAULT '',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS job_applications (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    job_requisition_id uuid NOT NULL,
    candidate_id uuid NOT NULL,
    stage varchar(64) NOT NULL DEFAULT 'Screening',
    status varchar(64) NOT NULL DEFAULT 'Active',
    applied_date date NOT NULL,
    screening_score numeric(5,2) NOT NULL DEFAULT 0,
    interview_score numeric(5,2) NOT NULL DEFAULT 0,
    offer_salary numeric(18,2) NOT NULL DEFAULT 0,
    reject_reason text NOT NULL DEFAULT '',
    owner_id uuid NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS interviews (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    application_id uuid NOT NULL,
    round integer NOT NULL DEFAULT 1,
    interview_type varchar(64) NOT NULL DEFAULT '',
    scheduled_at timestamptz NOT NULL,
    duration_minutes integer NOT NULL DEFAULT 60,
    interviewers text NOT NULL DEFAULT '',
    location_or_link text NOT NULL DEFAULT '',
    result varchar(64) NOT NULL DEFAULT 'Pending',
    score numeric(5,2) NOT NULL DEFAULT 0,
    feedback text NOT NULL DEFAULT '',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS offers (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    application_id uuid NOT NULL,
    offer_no varchar(64) NOT NULL,
    status varchar(64) NOT NULL DEFAULT 'Draft',
    offered_salary numeric(18,2) NOT NULL DEFAULT 0,
    currency varchar(16) NOT NULL DEFAULT 'VND',
    start_date date NULL,
    offer_file_id uuid NULL,
    sent_at timestamptz NULL,
    accepted_at timestamptz NULL,
    rejected_at timestamptz NULL,
    reject_reason text NOT NULL DEFAULT '',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS onboarding_checklists (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    employee_id uuid NOT NULL,
    offer_id uuid NULL,
    checklist_no varchar(64) NOT NULL,
    status varchar(64) NOT NULL DEFAULT 'Open',
    items_json jsonb NOT NULL DEFAULT '[]',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_employees_tenant_code ON employees(tenant_id, employee_code);
CREATE UNIQUE INDEX IF NOT EXISTS ux_departments_tenant_code ON departments(tenant_id, department_code);
CREATE UNIQUE INDEX IF NOT EXISTS ux_positions_tenant_code ON positions(tenant_id, position_code);
CREATE UNIQUE INDEX IF NOT EXISTS ux_employee_contracts_tenant_no ON employee_contracts(tenant_id, contract_no);
CREATE UNIQUE INDEX IF NOT EXISTS ux_job_requisitions_tenant_no ON job_requisitions(tenant_id, requisition_no);
CREATE UNIQUE INDEX IF NOT EXISTS ux_candidates_tenant_code ON candidates(tenant_id, candidate_code);
CREATE UNIQUE INDEX IF NOT EXISTS ux_offers_tenant_no ON offers(tenant_id, offer_no);
