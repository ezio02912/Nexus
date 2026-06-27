CREATE TABLE IF NOT EXISTS products (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    product_code varchar(64) NOT NULL,
    product_name varchar(256) NOT NULL,
    unit varchar(16) NOT NULL,
    category varchar(128) NOT NULL,
    price numeric(18,2) NOT NULL,
    tax_percent numeric(5,2) NOT NULL,
    is_active boolean NOT NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_products_tenant_code
    ON products (tenant_id, product_code);

CREATE TABLE IF NOT EXISTS warehouses (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    warehouse_code varchar(64) NOT NULL,
    name varchar(256) NOT NULL,
    location varchar(256) NOT NULL,
    is_active boolean NOT NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_warehouses_tenant_code
    ON warehouses (tenant_id, warehouse_code);
