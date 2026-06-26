CREATE TABLE IF NOT EXISTS sales_orders (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    customer_id uuid NOT NULL,
    order_no varchar(64) NOT NULL,
    status varchar(32) NOT NULL,
    total_amount numeric(18,2) NOT NULL,
    created_at timestamptz NOT NULL,
    approved_at timestamptz NULL,
    completed_at timestamptz NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_sales_orders_tenant_no ON sales_orders (tenant_id, order_no);
CREATE INDEX IF NOT EXISTS ix_sales_orders_tenant_customer ON sales_orders (tenant_id, customer_id);

CREATE TABLE IF NOT EXISTS sales_order_lines (
    id uuid PRIMARY KEY,
    sales_order_id uuid NOT NULL REFERENCES sales_orders(id) ON DELETE CASCADE,
    product_code varchar(64) NOT NULL,
    description varchar(256) NOT NULL,
    quantity numeric(18,2) NOT NULL,
    unit_price numeric(18,2) NOT NULL,
    line_amount numeric(18,2) NOT NULL
);

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
