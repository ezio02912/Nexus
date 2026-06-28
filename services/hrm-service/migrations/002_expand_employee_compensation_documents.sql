ALTER TABLE employees ADD COLUMN IF NOT EXISTS job_level varchar(64) NOT NULL DEFAULT '';
ALTER TABLE employees ADD COLUMN IF NOT EXISTS job_grade varchar(64) NOT NULL DEFAULT '';
ALTER TABLE employees ADD COLUMN IF NOT EXISTS probation_salary numeric(18,2) NOT NULL DEFAULT 0;
ALTER TABLE employees ADD COLUMN IF NOT EXISTS official_salary numeric(18,2) NOT NULL DEFAULT 0;
ALTER TABLE employees ADD COLUMN IF NOT EXISTS performance_bonus_percent numeric(6,2) NOT NULL DEFAULT 0;

CREATE TABLE IF NOT EXISTS employee_allowances (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    employee_id uuid NOT NULL,
    allowance_type varchar(64) NOT NULL DEFAULT '',
    name varchar(256) NOT NULL,
    amount numeric(18,2) NOT NULL DEFAULT 0,
    currency varchar(16) NOT NULL DEFAULT 'VND',
    taxable boolean NOT NULL DEFAULT true,
    insurance_included boolean NOT NULL DEFAULT false,
    effective_from date NULL,
    effective_to date NULL,
    status varchar(64) NOT NULL DEFAULT 'Active',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS employee_benefits (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    employee_id uuid NOT NULL,
    benefit_type varchar(64) NOT NULL DEFAULT '',
    name varchar(256) NOT NULL,
    policy_code varchar(64) NOT NULL DEFAULT '',
    start_date date NULL,
    end_date date NULL,
    status varchar(64) NOT NULL DEFAULT 'Active',
    notes text NOT NULL DEFAULT '',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS employee_documents (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    employee_id uuid NOT NULL,
    document_type varchar(64) NOT NULL DEFAULT '',
    file_id uuid NOT NULL,
    file_name varchar(512) NOT NULL DEFAULT '',
    issued_date date NULL,
    expired_date date NULL,
    status varchar(64) NOT NULL DEFAULT 'Active',
    notes text NOT NULL DEFAULT '',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE INDEX IF NOT EXISTS ix_employee_allowances_employee ON employee_allowances(tenant_id, employee_id);
CREATE INDEX IF NOT EXISTS ix_employee_benefits_employee ON employee_benefits(tenant_id, employee_id);
CREATE INDEX IF NOT EXISTS ix_employee_documents_employee ON employee_documents(tenant_id, employee_id);
