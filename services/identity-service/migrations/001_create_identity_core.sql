CREATE TABLE IF NOT EXISTS users (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    user_name varchar(64) NOT NULL,
    email varchar(256) NOT NULL,
    password_hash text NOT NULL,
    is_active boolean NOT NULL DEFAULT true,
    creation_time timestamptz NOT NULL,
    creator_id uuid NULL,
    last_modification_time timestamptz NULL,
    last_modifier_id uuid NULL,
    is_deleted boolean NOT NULL DEFAULT false,
    deletion_time timestamptz NULL,
    deleter_id uuid NULL,
    concurrency_stamp varchar(64) NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_users_tenant_user_name ON users (tenant_id, user_name) WHERE is_deleted = false;
CREATE UNIQUE INDEX IF NOT EXISTS ux_users_tenant_email ON users (tenant_id, email) WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS ix_users_tenant_id ON users (tenant_id);

CREATE TABLE IF NOT EXISTS user_roles (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_name varchar(64) NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_user_roles_user_role ON user_roles (user_id, role_name);

CREATE TABLE IF NOT EXISTS refresh_tokens (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash text NOT NULL,
    expires_at timestamptz NOT NULL,
    revoked_at timestamptz NULL,
    created_at timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_refresh_tokens_user_id ON refresh_tokens (user_id);

CREATE TABLE IF NOT EXISTS identity_outbox_messages (
    event_id uuid PRIMARY KEY,
    event_name varchar(256) NOT NULL,
    tenant_id uuid NULL,
    source_service varchar(128) NOT NULL,
    payload_json jsonb NOT NULL,
    occurred_at timestamptz NOT NULL,
    published_at timestamptz NULL,
    error text NULL
);

CREATE INDEX IF NOT EXISTS ix_identity_outbox_unpublished ON identity_outbox_messages (occurred_at) WHERE published_at IS NULL;
