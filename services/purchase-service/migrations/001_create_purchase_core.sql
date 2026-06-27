CREATE TABLE IF NOT EXISTS suppliers (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    supplier_code varchar(64) NOT NULL,
    name varchar(256) NOT NULL,
    email varchar(256) NOT NULL,
    phone varchar(64) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_suppliers_tenant_code ON suppliers (tenant_id, supplier_code);

CREATE TABLE IF NOT EXISTS purchase_orders (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    purchase_order_no varchar(64) NOT NULL,
    supplier_code varchar(64) NOT NULL,
    supplier_name varchar(256) NOT NULL,
    status varchar(32) NOT NULL,
    total_amount numeric(18,2) NOT NULL,
    created_at timestamptz NOT NULL,
    approved_at timestamptz NULL,
    received_at timestamptz NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_purchase_orders_tenant_no ON purchase_orders (tenant_id, purchase_order_no);

CREATE TABLE IF NOT EXISTS purchase_order_lines (
    id uuid PRIMARY KEY,
    purchase_order_id uuid NOT NULL REFERENCES purchase_orders(id) ON DELETE CASCADE,
    warehouse_code varchar(64) NOT NULL,
    product_code varchar(64) NOT NULL,
    product_name varchar(256) NOT NULL,
    quantity numeric(18,2) NOT NULL,
    unit_cost numeric(18,2) NOT NULL,
    line_amount numeric(18,2) NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_purchase_order_lines_order ON purchase_order_lines (purchase_order_id);

CREATE TABLE IF NOT EXISTS goods_receipts (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    purchase_order_id uuid NOT NULL,
    purchase_order_no varchar(64) NOT NULL,
    receipt_no varchar(64) NOT NULL,
    received_at timestamptz NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_goods_receipts_tenant_no ON goods_receipts (tenant_id, receipt_no);

CREATE TABLE IF NOT EXISTS goods_receipt_lines (
    id uuid PRIMARY KEY,
    goods_receipt_id uuid NOT NULL REFERENCES goods_receipts(id) ON DELETE CASCADE,
    warehouse_code varchar(64) NOT NULL,
    product_code varchar(64) NOT NULL,
    product_name varchar(256) NOT NULL,
    quantity numeric(18,2) NOT NULL,
    unit_cost numeric(18,2) NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_goods_receipt_lines_receipt ON goods_receipt_lines (goods_receipt_id);

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
