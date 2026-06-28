ALTER TABLE crm_activities
    ADD COLUMN IF NOT EXISTS assigned_to_ids varchar(2048) NOT NULL DEFAULT '';

UPDATE crm_activities
SET assigned_to_ids = assigned_to_id::text
WHERE assigned_to_id IS NOT NULL
  AND assigned_to_ids = '';
