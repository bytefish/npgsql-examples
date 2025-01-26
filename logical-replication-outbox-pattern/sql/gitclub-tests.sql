-- Performs a Cleanup for Tests, which removes all data except the Data Conversion User
CREATE OR REPLACE PROCEDURE gitclub.cleanup_tests()
AS $cleanup_tests_func$
BEGIN

	-- Delete all non-fixed data
	DELETE FROM gitclub.issue;
	DELETE FROM gitclub.repository;
	DELETE FROM gitclub.team;
	DELETE FROM gitclub.organization;
	DELETE FROM gitclub.outbox_event;
	DELETE FROM gitclub.user WHERE user_id != 1;
    
END; $cleanup_tests_func$ 
LANGUAGE plpgsql;
