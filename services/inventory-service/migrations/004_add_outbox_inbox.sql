-- Adds the transactional outbox/inbox tables used by the messaging building block.
-- The inventory service's OutboxDispatcherHostedService polls outbox_messages every few
-- seconds; without this table it logs "relation \"outbox_messages\" does not exist".

CREATE TABLE IF NOT EXISTS outbox_messages (
    event_id uuid PRIMARY KEY,
    event_name varchar(256) NOT NULL,
    tenant_id uuid NULL,
    source_service varchar(128) NOT NULL,
    payload_json jsonb NOT NULL,
    occurred_at timestamptz NOT NULL,
    published_at timestamptz NULL,
    error text NULL
);

CREATE INDEX IF NOT EXISTS ix_outbox_unpublished ON outbox_messages (occurred_at) WHERE published_at IS NULL;

CREATE TABLE IF NOT EXISTS inbox_messages (
    event_id uuid PRIMARY KEY,
    event_name varchar(256) NOT NULL,
    source_service varchar(128) NOT NULL,
    payload_json jsonb NOT NULL,
    received_at timestamptz NOT NULL,
    processed_at timestamptz NULL,
    error text NULL
);
