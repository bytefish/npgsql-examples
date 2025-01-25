CREATE OR REPLACE FUNCTION notify_trigger() 
RETURNS TRIGGER AS $trigger$
DECLARE
  rec RECORD;
  payload TEXT;
  channel_name TEXT;
  payload_items json;
BEGIN

  -- Get the Operation
  CASE TG_OP
  WHEN 'INSERT','UPDATE' THEN
     rec := NEW;
  WHEN 'DELETE' THEN
     rec := OLD;
  ELSE
	RAISE EXCEPTION 'Unknown TG_OP: "%". Should not occur!', TG_OP;
  END CASE;
  
  -- Get the Channel Name
  IF TG_ARGV[0] IS NULL THEN
	RAISE EXCEPTION 'A Channel Name is required as first argument';
  END IF;
  
  channel_name := TG_ARGV[0];
  
  -- Get the payload
  payload_items := row_to_json(rec);

  -- Build the payload
  payload := json_build_object(
      'timestamp', CURRENT_TIMESTAMP
    , 'row_version', rec.xmin
	, 'operation', TG_OP
    , 'schema', TG_TABLE_SCHEMA
    , 'table', TG_TABLE_NAME
    , 'payload', payload_items);

  -- Notify the channel
  PERFORM pg_notify(channel_name, payload);
  
  RETURN rec;
END;
$trigger$ LANGUAGE plpgsql;

-- Triggers (Notifications)
CREATE OR REPLACE TRIGGER user_notify_trigger
AFTER INSERT OR UPDATE OR DELETE ON gitclub.user
FOR EACH ROW EXECUTE PROCEDURE notify_trigger('core_db_event');

CREATE OR REPLACE TRIGGER organization_notify_trigger
AFTER INSERT OR UPDATE OR DELETE ON gitclub.organization
FOR EACH ROW EXECUTE PROCEDURE notify_trigger('core_db_event');

CREATE OR REPLACE TRIGGER repository_notify_trigger
AFTER INSERT OR UPDATE OR DELETE ON gitclub.repository
FOR EACH ROW EXECUTE PROCEDURE notify_trigger('core_db_event');

CREATE OR REPLACE TRIGGER issue_notify_trigger
AFTER INSERT OR UPDATE OR DELETE ON gitclub.issue
FOR EACH ROW EXECUTE PROCEDURE notify_trigger('core_db_event');

CREATE OR REPLACE TRIGGER team_notify_trigger
AFTER INSERT OR UPDATE OR DELETE ON gitclub.team
FOR EACH ROW EXECUTE PROCEDURE notify_trigger('core_db_event');