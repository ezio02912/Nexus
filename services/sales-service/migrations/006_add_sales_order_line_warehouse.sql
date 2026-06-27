ALTER TABLE sales_order_lines
    ADD COLUMN IF NOT EXISTS warehouse_code varchar(64) NOT NULL DEFAULT 'MAIN';

UPDATE sales_order_lines
SET warehouse_code = 'MAIN'
WHERE warehouse_code IS NULL OR warehouse_code = '';
