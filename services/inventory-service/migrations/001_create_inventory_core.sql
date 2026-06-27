CREATE TABLE IF NOT EXISTS stock_balances (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    warehouse_code varchar(64) NOT NULL,
    product_code varchar(64) NOT NULL,
    product_name varchar(256) NOT NULL,
    on_hand_quantity numeric(18,2) NOT NULL,
    reserved_quantity numeric(18,2) NOT NULL,
    updated_at timestamptz NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_stock_balances_tenant_warehouse_product
    ON stock_balances (tenant_id, warehouse_code, product_code);

CREATE TABLE IF NOT EXISTS stock_reservations (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    source_type varchar(32) NOT NULL,
    source_id uuid NOT NULL,
    source_no varchar(64) NOT NULL,
    status varchar(32) NOT NULL,
    created_at timestamptz NOT NULL,
    shipped_at timestamptz NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_stock_reservations_tenant_source
    ON stock_reservations (tenant_id, source_type, source_id);

CREATE TABLE IF NOT EXISTS stock_reservation_lines (
    id uuid PRIMARY KEY,
    reservation_id uuid NOT NULL REFERENCES stock_reservations(id) ON DELETE CASCADE,
    warehouse_code varchar(64) NOT NULL,
    product_code varchar(64) NOT NULL,
    description varchar(256) NOT NULL,
    quantity numeric(18,2) NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_stock_reservation_lines_reservation
    ON stock_reservation_lines (reservation_id);

CREATE TABLE IF NOT EXISTS stock_movements (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    warehouse_code varchar(64) NOT NULL,
    product_code varchar(64) NOT NULL,
    movement_type varchar(32) NOT NULL,
    quantity numeric(18,2) NOT NULL,
    source_type varchar(32) NOT NULL,
    source_id uuid NOT NULL,
    source_no varchar(64) NOT NULL,
    occurred_at timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_stock_movements_tenant_product_time
    ON stock_movements (tenant_id, product_code, occurred_at);

CREATE INDEX IF NOT EXISTS ix_stock_movements_tenant_source
    ON stock_movements (tenant_id, source_type, source_id);
