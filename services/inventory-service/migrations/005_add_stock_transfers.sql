-- Inter-warehouse stock transfers are now first-class documents (numbered TR-...),
-- so we can attach a transfer note and trace the two stock movements they create.

CREATE TABLE IF NOT EXISTS stock_transfers (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    transfer_no varchar(64) NOT NULL,
    from_warehouse_code varchar(64) NOT NULL,
    to_warehouse_code varchar(64) NOT NULL,
    product_code varchar(64) NOT NULL,
    product_name varchar(256) NOT NULL,
    quantity numeric(18,2) NOT NULL,
    status varchar(32) NOT NULL,
    created_at timestamptz NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_stock_transfers_tenant_no
    ON stock_transfers (tenant_id, transfer_no);

CREATE INDEX IF NOT EXISTS ix_stock_transfers_tenant_created
    ON stock_transfers (tenant_id, created_at);
