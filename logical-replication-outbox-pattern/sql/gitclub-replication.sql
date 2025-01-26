DO $$

BEGIN

IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_publication WHERE pubname = 'outbox_pub') 
THEN
    CREATE PUBLICATION outbox_pub FOR TABLE 
        gitclub.outbox_event;
END IF;

END;
$$ LANGUAGE plpgsql;

SELECT 'outbox_slot_init' FROM pg_create_logical_replication_slot('outbox_slot', 'pgoutput');

