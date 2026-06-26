ALTER TABLE sales_orders
    ADD COLUMN IF NOT EXISTS source_type varchar(32) NULL,
    ADD COLUMN IF NOT EXISTS source_id uuid NULL,
    ADD COLUMN IF NOT EXISTS source_no varchar(64) NULL;

CREATE INDEX IF NOT EXISTS ix_sales_orders_tenant_source
    ON sales_orders (tenant_id, source_type, source_id);
