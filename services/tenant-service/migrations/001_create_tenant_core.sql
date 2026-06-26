CREATE TABLE IF NOT EXISTS tenants (
    id uuid PRIMARY KEY,
    code varchar(64) NOT NULL,
    name varchar(256) NOT NULL,
    status varchar(32) NOT NULL,
    creation_time timestamptz NOT NULL,
    creator_id uuid NULL,
    last_modification_time timestamptz NULL,
    last_modifier_id uuid NULL,
    is_deleted boolean NOT NULL DEFAULT false,
    deletion_time timestamptz NULL,
    deleter_id uuid NULL,
    concurrency_stamp varchar(64) NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_tenants_code ON tenants (code) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS ix_tenants_status ON tenants (status);

CREATE TABLE IF NOT EXISTS tenant_subscriptions (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    plan_code varchar(64) NOT NULL,
    expires_at timestamptz NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_tenant_subscriptions_tenant_id ON tenant_subscriptions (tenant_id);

CREATE TABLE IF NOT EXISTS tenant_modules (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    module_code varchar(64) NOT NULL,
    is_enabled boolean NOT NULL DEFAULT true
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_tenant_modules_tenant_module ON tenant_modules (tenant_id, module_code);

CREATE TABLE IF NOT EXISTS tenant_settings (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    key varchar(128) NOT NULL,
    value text NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_tenant_settings_tenant_key ON tenant_settings (tenant_id, key);

CREATE TABLE IF NOT EXISTS tenant_outbox_messages (
    event_id uuid PRIMARY KEY,
    event_name varchar(256) NOT NULL,
    tenant_id uuid NULL,
    source_service varchar(128) NOT NULL,
    payload_json jsonb NOT NULL,
    occurred_at timestamptz NOT NULL,
    published_at timestamptz NULL,
    error text NULL
);

CREATE INDEX IF NOT EXISTS ix_tenant_outbox_unpublished ON tenant_outbox_messages (occurred_at) WHERE published_at IS NULL;
