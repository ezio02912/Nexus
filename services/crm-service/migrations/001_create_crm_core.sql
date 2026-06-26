CREATE TABLE IF NOT EXISTS customers (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    code varchar(64) NOT NULL,
    name varchar(256) NOT NULL,
    email varchar(256) NULL,
    phone varchar(64) NULL,
    status varchar(32) NOT NULL,
    created_at timestamptz NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_customers_tenant_code ON customers (tenant_id, code);
CREATE INDEX IF NOT EXISTS ix_customers_tenant_name ON customers (tenant_id, name);

CREATE TABLE IF NOT EXISTS contacts (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    customer_id uuid NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    full_name varchar(256) NOT NULL,
    email varchar(256) NULL,
    phone varchar(64) NULL,
    position varchar(128) NULL,
    created_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS leads (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    full_name varchar(256) NOT NULL,
    company_name varchar(256) NULL,
    email varchar(256) NULL,
    phone varchar(64) NULL,
    source varchar(128) NULL,
    status varchar(32) NOT NULL,
    created_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS opportunities (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    customer_id uuid NULL REFERENCES customers(id) ON DELETE SET NULL,
    name varchar(256) NOT NULL,
    stage varchar(64) NOT NULL,
    amount numeric(18,2) NOT NULL DEFAULT 0,
    expected_close_date date NULL,
    created_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS quotations (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    customer_id uuid NOT NULL REFERENCES customers(id) ON DELETE RESTRICT,
    quotation_no varchar(64) NOT NULL,
    total_amount numeric(18,2) NOT NULL,
    status varchar(32) NOT NULL,
    created_at timestamptz NOT NULL,
    approved_at timestamptz NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_quotations_tenant_no ON quotations (tenant_id, quotation_no);

CREATE TABLE IF NOT EXISTS contracts (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    customer_id uuid NOT NULL REFERENCES customers(id) ON DELETE RESTRICT,
    contract_no varchar(64) NOT NULL,
    title varchar(256) NOT NULL,
    status varchar(32) NOT NULL,
    signed_at timestamptz NULL,
    created_at timestamptz NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_contracts_tenant_no ON contracts (tenant_id, contract_no);

CREATE TABLE IF NOT EXISTS outbox_messages (
    event_id uuid PRIMARY KEY,
    event_name varchar(256) NOT NULL,
    tenant_id uuid NULL,
    source_service varchar(128) NOT NULL,
    payload_json jsonb NOT NULL,
    occurred_at timestamptz NOT NULL,
    published_at timestamptz NULL,
    error text NULL
);

CREATE INDEX IF NOT EXISTS ix_outbox_unpublished ON outbox_messages (occurred_at) WHERE published_at IS NULL;

CREATE TABLE IF NOT EXISTS inbox_messages (
    event_id uuid PRIMARY KEY,
    event_name varchar(256) NOT NULL,
    source_service varchar(128) NOT NULL,
    payload_json jsonb NOT NULL,
    received_at timestamptz NOT NULL,
    processed_at timestamptz NULL,
    error text NULL
);
