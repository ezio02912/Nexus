-- CRM Enterprise schema expansion (002)
-- Migrates from MVP schema (created_at) to FullAudited schema (creation_time + audit columns)

-- Customers expansion
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'customers' AND column_name = 'created_at'
    ) AND NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'customers' AND column_name = 'creation_time'
    ) THEN
        ALTER TABLE customers RENAME COLUMN created_at TO creation_time;
    END IF;
END $$;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS creation_time timestamptz NOT NULL DEFAULT now();
ALTER TABLE customers ALTER COLUMN creation_time SET DEFAULT now();
UPDATE customers SET creation_time = now() WHERE creation_time IS NULL;
ALTER TABLE customers ALTER COLUMN creation_time SET NOT NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS customer_type varchar(32) NOT NULL DEFAULT 'Company';
ALTER TABLE customers ADD COLUMN IF NOT EXISTS tax_code varchar(32) NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS website varchar(256) NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS industry varchar(128) NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS address_line1 varchar(256) NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS address_line2 varchar(256) NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS city varchar(128) NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS state varchar(128) NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS postal_code varchar(32) NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS country varchar(64) NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS owner_id uuid NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS description text NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS rating varchar(16) NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS source varchar(128) NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS creator_id uuid NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS last_modification_time timestamptz NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS last_modifier_id uuid NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS is_deleted boolean NOT NULL DEFAULT false;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS deletion_time timestamptz NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS deleter_id uuid NULL;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS concurrency_stamp varchar(64) NOT NULL DEFAULT '';
CREATE INDEX IF NOT EXISTS ix_customers_tenant_tax_code ON customers (tenant_id, tax_code) WHERE tax_code IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_customers_tenant_owner ON customers (tenant_id, owner_id) WHERE owner_id IS NOT NULL;

-- Contacts expansion
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'contacts' AND column_name = 'created_at'
    ) AND NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'contacts' AND column_name = 'creation_time'
    ) THEN
        ALTER TABLE contacts RENAME COLUMN created_at TO creation_time;
    END IF;
END $$;
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS creation_time timestamptz NOT NULL DEFAULT now();
ALTER TABLE contacts ALTER COLUMN creation_time SET DEFAULT now();
UPDATE contacts SET creation_time = now() WHERE creation_time IS NULL;
ALTER TABLE contacts ALTER COLUMN creation_time SET NOT NULL;
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS mobile varchar(64) NULL;
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS department varchar(128) NULL;
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS is_primary boolean NOT NULL DEFAULT false;
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS is_decision_maker boolean NOT NULL DEFAULT false;
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS linkedin_url varchar(512) NULL;
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS notes text NULL;
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS owner_id uuid NULL;
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS creator_id uuid NULL;
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS last_modification_time timestamptz NULL;
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS last_modifier_id uuid NULL;
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS is_deleted boolean NOT NULL DEFAULT false;
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS deletion_time timestamptz NULL;
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS deleter_id uuid NULL;
ALTER TABLE contacts ADD COLUMN IF NOT EXISTS concurrency_stamp varchar(64) NOT NULL DEFAULT '';

-- Leads expansion
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'leads' AND column_name = 'created_at'
    ) AND NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'leads' AND column_name = 'creation_time'
    ) THEN
        ALTER TABLE leads RENAME COLUMN created_at TO creation_time;
    END IF;
END $$;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS creation_time timestamptz NOT NULL DEFAULT now();
ALTER TABLE leads ALTER COLUMN creation_time SET DEFAULT now();
UPDATE leads SET creation_time = now() WHERE creation_time IS NULL;
ALTER TABLE leads ALTER COLUMN creation_time SET NOT NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS title varchar(128) NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS lead_score int NOT NULL DEFAULT 0;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS rating varchar(16) NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS owner_id uuid NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS assigned_at timestamptz NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS converted_customer_id uuid NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS converted_opportunity_id uuid NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS converted_at timestamptz NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS lost_reason text NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS description text NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS address varchar(256) NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS city varchar(128) NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS country varchar(64) NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS creator_id uuid NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS last_modification_time timestamptz NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS last_modifier_id uuid NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS is_deleted boolean NOT NULL DEFAULT false;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS deletion_time timestamptz NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS deleter_id uuid NULL;
ALTER TABLE leads ADD COLUMN IF NOT EXISTS concurrency_stamp varchar(64) NOT NULL DEFAULT '';

-- Opportunities expansion
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'opportunities' AND column_name = 'created_at'
    ) AND NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'opportunities' AND column_name = 'creation_time'
    ) THEN
        ALTER TABLE opportunities RENAME COLUMN created_at TO creation_time;
    END IF;
END $$;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS creation_time timestamptz NOT NULL DEFAULT now();
ALTER TABLE opportunities ALTER COLUMN creation_time SET DEFAULT now();
UPDATE opportunities SET creation_time = now() WHERE creation_time IS NULL;
ALTER TABLE opportunities ALTER COLUMN creation_time SET NOT NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS lead_id uuid NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS contact_id uuid NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS owner_id uuid NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS probability int NOT NULL DEFAULT 10;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS currency varchar(3) NOT NULL DEFAULT 'VND';
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS actual_close_date date NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS close_reason text NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS lost_reason text NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS description text NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS next_step varchar(512) NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS next_step_date date NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS source varchar(128) NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS competitor varchar(256) NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS creator_id uuid NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS last_modification_time timestamptz NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS last_modifier_id uuid NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS is_deleted boolean NOT NULL DEFAULT false;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS deletion_time timestamptz NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS deleter_id uuid NULL;
ALTER TABLE opportunities ADD COLUMN IF NOT EXISTS concurrency_stamp varchar(64) NOT NULL DEFAULT '';

-- Quotations expansion
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'quotations' AND column_name = 'created_at'
    ) AND NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'quotations' AND column_name = 'creation_time'
    ) THEN
        ALTER TABLE quotations RENAME COLUMN created_at TO creation_time;
    END IF;
END $$;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS creation_time timestamptz NOT NULL DEFAULT now();
ALTER TABLE quotations ALTER COLUMN creation_time SET DEFAULT now();
UPDATE quotations SET creation_time = now() WHERE creation_time IS NULL;
ALTER TABLE quotations ALTER COLUMN creation_time SET NOT NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS opportunity_id uuid NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS contact_id uuid NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS owner_id uuid NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS subject varchar(256) NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS description text NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS subtotal numeric(18,2) NOT NULL DEFAULT 0;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS discount_amount numeric(18,2) NOT NULL DEFAULT 0;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS discount_percent numeric(5,2) NOT NULL DEFAULT 0;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS tax_amount numeric(18,2) NOT NULL DEFAULT 0;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS currency varchar(3) NOT NULL DEFAULT 'VND';
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS valid_until date NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS approved_by uuid NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS rejected_at timestamptz NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS rejection_reason text NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS notes text NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS terms text NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS creator_id uuid NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS last_modification_time timestamptz NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS last_modifier_id uuid NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS is_deleted boolean NOT NULL DEFAULT false;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS deletion_time timestamptz NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS deleter_id uuid NULL;
ALTER TABLE quotations ADD COLUMN IF NOT EXISTS concurrency_stamp varchar(64) NOT NULL DEFAULT '';

CREATE TABLE IF NOT EXISTS quotation_lines (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    quotation_id uuid NOT NULL REFERENCES quotations(id) ON DELETE CASCADE,
    line_no int NOT NULL,
    product_code varchar(64) NOT NULL,
    product_name varchar(256) NOT NULL,
    description text NULL,
    quantity numeric(18,4) NOT NULL DEFAULT 1,
    unit varchar(32) NOT NULL DEFAULT 'EA',
    unit_price numeric(18,2) NOT NULL DEFAULT 0,
    discount_percent numeric(5,2) NOT NULL DEFAULT 0,
    tax_percent numeric(5,2) NOT NULL DEFAULT 0,
    line_total numeric(18,2) NOT NULL DEFAULT 0,
    sort_order int NOT NULL DEFAULT 0
);
CREATE INDEX IF NOT EXISTS ix_quotation_lines_quotation ON quotation_lines (quotation_id);

-- Contracts expansion
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'contracts' AND column_name = 'created_at'
    ) AND NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'contracts' AND column_name = 'creation_time'
    ) THEN
        ALTER TABLE contracts RENAME COLUMN created_at TO creation_time;
    END IF;
END $$;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS creation_time timestamptz NOT NULL DEFAULT now();
ALTER TABLE contracts ALTER COLUMN creation_time SET DEFAULT now();
UPDATE contracts SET creation_time = now() WHERE creation_time IS NULL;
ALTER TABLE contracts ALTER COLUMN creation_time SET NOT NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS quotation_id uuid NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS opportunity_id uuid NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS contact_id uuid NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS owner_id uuid NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS contract_value numeric(18,2) NOT NULL DEFAULT 0;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS currency varchar(3) NOT NULL DEFAULT 'VND';
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS start_date date NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS end_date date NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS renewal_date date NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS signed_by uuid NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS termination_reason text NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS payment_terms text NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS notes text NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS terms text NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS file_id uuid NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS creator_id uuid NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS last_modification_time timestamptz NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS last_modifier_id uuid NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS is_deleted boolean NOT NULL DEFAULT false;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS deletion_time timestamptz NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS deleter_id uuid NULL;
ALTER TABLE contracts ADD COLUMN IF NOT EXISTS concurrency_stamp varchar(64) NOT NULL DEFAULT '';

CREATE TABLE IF NOT EXISTS contract_lines (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    contract_id uuid NOT NULL REFERENCES contracts(id) ON DELETE CASCADE,
    line_no int NOT NULL,
    product_code varchar(64) NOT NULL,
    product_name varchar(256) NOT NULL,
    description text NULL,
    quantity numeric(18,4) NOT NULL DEFAULT 1,
    unit varchar(32) NOT NULL DEFAULT 'EA',
    unit_price numeric(18,2) NOT NULL DEFAULT 0,
    discount_percent numeric(5,2) NOT NULL DEFAULT 0,
    tax_percent numeric(5,2) NOT NULL DEFAULT 0,
    line_total numeric(18,2) NOT NULL DEFAULT 0,
    sort_order int NOT NULL DEFAULT 0
);
CREATE INDEX IF NOT EXISTS ix_contract_lines_contract ON contract_lines (contract_id);

-- CRM Activities
CREATE TABLE IF NOT EXISTS crm_activities (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    related_entity_type varchar(32) NOT NULL,
    related_entity_id uuid NOT NULL,
    activity_type varchar(32) NOT NULL,
    subject varchar(256) NOT NULL,
    description text NULL,
    activity_date timestamptz NOT NULL,
    due_date timestamptz NULL,
    completed_at timestamptz NULL,
    status varchar(32) NOT NULL,
    owner_id uuid NULL,
    assigned_to_id uuid NULL,
    duration_minutes int NULL,
    creation_time timestamptz NOT NULL,
    creator_id uuid NULL,
    last_modification_time timestamptz NULL,
    last_modifier_id uuid NULL,
    is_deleted boolean NOT NULL DEFAULT false,
    deletion_time timestamptz NULL,
    deleter_id uuid NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);
CREATE INDEX IF NOT EXISTS ix_crm_activities_tenant ON crm_activities (tenant_id);
CREATE INDEX IF NOT EXISTS ix_crm_activities_related ON crm_activities (tenant_id, related_entity_type, related_entity_id);

-- Pipeline stages
CREATE TABLE IF NOT EXISTS pipeline_stages (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    entity_type varchar(64) NOT NULL,
    code varchar(64) NOT NULL,
    name varchar(128) NOT NULL,
    sort_order int NOT NULL DEFAULT 0,
    probability_default int NOT NULL DEFAULT 0,
    is_won boolean NOT NULL DEFAULT false,
    is_lost boolean NOT NULL DEFAULT false,
    is_active boolean NOT NULL DEFAULT true,
    creation_time timestamptz NOT NULL,
    creator_id uuid NULL,
    last_modification_time timestamptz NULL,
    last_modifier_id uuid NULL,
    is_deleted boolean NOT NULL DEFAULT false,
    deletion_time timestamptz NULL,
    deleter_id uuid NULL,
    concurrency_stamp varchar(64) NOT NULL DEFAULT ''
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_pipeline_stages_tenant_code ON pipeline_stages (tenant_id, entity_type, code);
