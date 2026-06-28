CREATE TABLE IF NOT EXISTS payroll_policies (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    policy_code varchar(64) NOT NULL,
    name varchar(256) NOT NULL,
    country_code varchar(16) NOT NULL DEFAULT 'VN',
    effective_from date NOT NULL,
    effective_to date NULL,
    social_insurance_employee_rate numeric(6,3) NOT NULL DEFAULT 8,
    health_insurance_employee_rate numeric(6,3) NOT NULL DEFAULT 1.5,
    unemployment_insurance_employee_rate numeric(6,3) NOT NULL DEFAULT 1,
    social_insurance_employer_rate numeric(6,3) NOT NULL DEFAULT 17.5,
    health_insurance_employer_rate numeric(6,3) NOT NULL DEFAULT 3,
    unemployment_insurance_employer_rate numeric(6,3) NOT NULL DEFAULT 1,
    union_fee_rate numeric(6,3) NOT NULL DEFAULT 2,
    personal_deduction_amount numeric(18,2) NOT NULL DEFAULT 11000000,
    dependent_deduction_amount numeric(18,2) NOT NULL DEFAULT 4400000,
    status varchar(64) NOT NULL DEFAULT 'Active',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS salary_components (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    component_code varchar(64) NOT NULL,
    name varchar(256) NOT NULL,
    component_type varchar(64) NOT NULL,
    taxable boolean NOT NULL DEFAULT true,
    insurance_included boolean NOT NULL DEFAULT false,
    recurring boolean NOT NULL DEFAULT true,
    formula text NOT NULL DEFAULT '',
    display_order integer NOT NULL DEFAULT 0,
    status varchar(64) NOT NULL DEFAULT 'Active',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS payroll_periods (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    period_code varchar(64) NOT NULL,
    month integer NOT NULL,
    year integer NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    payment_date date NOT NULL,
    status varchar(64) NOT NULL DEFAULT 'Open',
    locked_at timestamptz NULL,
    locked_by uuid NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS payroll_runs (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    run_no varchar(64) NOT NULL,
    period_id uuid NOT NULL,
    payroll_group_id uuid NULL,
    status varchar(64) NOT NULL DEFAULT 'Draft',
    total_gross numeric(18,2) NOT NULL DEFAULT 0,
    total_insurance_employee numeric(18,2) NOT NULL DEFAULT 0,
    total_taxable_income numeric(18,2) NOT NULL DEFAULT 0,
    total_pit numeric(18,2) NOT NULL DEFAULT 0,
    total_net_pay numeric(18,2) NOT NULL DEFAULT 0,
    calculated_at timestamptz NULL,
    approved_by uuid NULL,
    approved_at timestamptz NULL,
    paid_at timestamptz NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS payroll_lines (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    payroll_run_id uuid NOT NULL,
    employee_id uuid NOT NULL,
    department_id uuid NULL,
    position_id uuid NULL,
    base_salary numeric(18,2) NOT NULL DEFAULT 0,
    working_days numeric(6,2) NOT NULL DEFAULT 0,
    paid_leave_days numeric(6,2) NOT NULL DEFAULT 0,
    unpaid_leave_days numeric(6,2) NOT NULL DEFAULT 0,
    absent_days numeric(6,2) NOT NULL DEFAULT 0,
    overtime_hours numeric(6,2) NOT NULL DEFAULT 0,
    gross_income numeric(18,2) NOT NULL DEFAULT 0,
    insurance_salary numeric(18,2) NOT NULL DEFAULT 0,
    employee_insurance_amount numeric(18,2) NOT NULL DEFAULT 0,
    employer_insurance_amount numeric(18,2) NOT NULL DEFAULT 0,
    taxable_income numeric(18,2) NOT NULL DEFAULT 0,
    personal_deduction numeric(18,2) NOT NULL DEFAULT 11000000,
    dependent_deduction numeric(18,2) NOT NULL DEFAULT 0,
    pit_amount numeric(18,2) NOT NULL DEFAULT 0,
    total_allowance numeric(18,2) NOT NULL DEFAULT 0,
    total_deduction numeric(18,2) NOT NULL DEFAULT 0,
    net_pay numeric(18,2) NOT NULL DEFAULT 0,
    payment_status varchar(64) NOT NULL DEFAULT 'Unpaid',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS payroll_line_components (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    payroll_line_id uuid NOT NULL,
    component_id uuid NOT NULL,
    amount numeric(18,2) NOT NULL DEFAULT 0,
    formula_result text NOT NULL DEFAULT '',
    taxable boolean NOT NULL DEFAULT true,
    insurance_included boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS payslips (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    payslip_no varchar(64) NOT NULL,
    payroll_line_id uuid NOT NULL,
    employee_id uuid NOT NULL,
    status varchar(64) NOT NULL DEFAULT 'Draft',
    published_at timestamptz NULL,
    viewed_at timestamptz NULL,
    file_id uuid NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS payroll_payments (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    payment_no varchar(64) NOT NULL,
    payroll_run_id uuid NOT NULL,
    employee_id uuid NOT NULL,
    payment_method varchar(64) NOT NULL DEFAULT 'BankTransfer',
    bank_account_no varchar(64) NOT NULL DEFAULT '',
    amount numeric(18,2) NOT NULL DEFAULT 0,
    status varchar(64) NOT NULL DEFAULT 'Pending',
    paid_at timestamptz NULL,
    reference_no varchar(128) NOT NULL DEFAULT '',
    failure_reason text NOT NULL DEFAULT '',
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_payroll_policies_tenant_code ON payroll_policies(tenant_id, policy_code);
CREATE UNIQUE INDEX IF NOT EXISTS ux_salary_components_tenant_code ON salary_components(tenant_id, component_code);
CREATE UNIQUE INDEX IF NOT EXISTS ux_payroll_periods_tenant_code ON payroll_periods(tenant_id, period_code);
CREATE UNIQUE INDEX IF NOT EXISTS ux_payroll_runs_tenant_no ON payroll_runs(tenant_id, run_no);
CREATE UNIQUE INDEX IF NOT EXISTS ux_payslips_tenant_no ON payslips(tenant_id, payslip_no);
CREATE UNIQUE INDEX IF NOT EXISTS ux_payroll_payments_tenant_no ON payroll_payments(tenant_id, payment_no);
