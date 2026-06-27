ALTER TABLE sales_orders
    ADD COLUMN IF NOT EXISTS delivery_status varchar(32) NOT NULL DEFAULT 'Pending',
    ADD COLUMN IF NOT EXISTS delivered_at timestamptz NULL;

UPDATE sales_orders
SET delivery_status = CASE
    WHEN completed_at IS NOT NULL THEN 'Delivered'
    WHEN approved_at IS NOT NULL THEN 'Ready'
    ELSE 'Pending'
END
WHERE delivery_status IS NULL OR delivery_status = 'Pending';
