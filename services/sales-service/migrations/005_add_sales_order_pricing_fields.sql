ALTER TABLE sales_orders
    ADD COLUMN IF NOT EXISTS subtotal numeric(18,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS discount_amount numeric(18,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS tax_amount numeric(18,2) NOT NULL DEFAULT 0;

ALTER TABLE sales_order_lines
    ADD COLUMN IF NOT EXISTS discount_percent numeric(5,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS discount_amount numeric(18,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS tax_percent numeric(5,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS tax_amount numeric(18,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS subtotal numeric(18,2) NOT NULL DEFAULT 0;

UPDATE sales_order_lines
SET subtotal = quantity * unit_price,
    discount_amount = 0,
    tax_amount = 0,
    line_amount = quantity * unit_price
WHERE subtotal = 0;

UPDATE sales_orders o
SET subtotal = COALESCE(s.subtotal, 0),
    discount_amount = COALESCE(s.discount_amount, 0),
    tax_amount = COALESCE(s.tax_amount, 0),
    total_amount = COALESCE(s.line_amount, 0)
FROM (
    SELECT sales_order_id,
           SUM(subtotal) AS subtotal,
           SUM(discount_amount) AS discount_amount,
           SUM(tax_amount) AS tax_amount,
           SUM(line_amount) AS line_amount
    FROM sales_order_lines
    GROUP BY sales_order_id
) s
WHERE o.id = s.sales_order_id;
