ALTER TABLE sales_orders
    ADD COLUMN IF NOT EXISTS inventory_reservation_status varchar(32) NOT NULL DEFAULT 'Pending',
    ADD COLUMN IF NOT EXISTS reserved_at timestamptz NULL;

UPDATE sales_orders
SET inventory_reservation_status = CASE
    WHEN approved_at IS NOT NULL THEN 'Reserved'
    ELSE 'Pending'
END
WHERE inventory_reservation_status IS NULL OR inventory_reservation_status = 'Pending';
