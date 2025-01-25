DO $$

BEGIN

-- Initial Data
INSERT INTO gitclub.user(user_id, email, preferred_name, last_edited_by) 
    VALUES 
        (1, 'philipp@bytefish.de', 'Data Conversion User', 1)        
    ON CONFLICT DO NOTHING;

-- OpenFGA GitHub Permissions Example (https://openfga.dev/docs/modeling/advanced/github)
INSERT INTO gitclub.user(user_id, email, preferred_name, last_edited_by) 
    VALUES 
        (2, 'anne@git.local', 'Anne', 1),
        (3, 'beth@git.local', 'Beth', 1),
        (4, 'charles@git.local', 'Charles', 1),
        (5, 'diane@git.local', 'Diane', 1),
        (6, 'erik@git.local', 'Eril', 1)        
    ON CONFLICT DO NOTHING;

INSERT INTO gitclub.organization(organization_id, name, billing_address, last_edited_by) 
    VALUES 
        (1, 'Contoso', 'ACME Street. 93', 1) 
    ON CONFLICT DO NOTHING; 

INSERT INTO gitclub.team(team_id, organization_id, name, last_edited_by) 
    VALUES 
        (1, 1, 'Engineering', 1),    -- A Team "Engineering" (1), that belongs to the "contoso" Organization (1)
        (2, 1, 'Protocols', 1)       -- A Team "Protocols" (1), that belongs to the "contoso" Organization (1)
    ON CONFLICT DO NOTHING;

INSERT INTO gitclub.repository(repository_id, organization_id, name, last_edited_by)
    VALUES
        (1, 1, 'Tooling', 1),    		-- A Repository "Tooling" (1), that belongs to the "Contoso" (1) organization
        (2, 1, 'Specifications', 1)		-- A Repository "Specifications" (2), that belongs to the "Contoso" (1) organization
    ON CONFLICT DO NOTHING;

INSERT INTO gitclub.issue(issue_id, title, content, closed, repository_id, created_by, last_edited_by)
    VALUES
        (1, 'GitClub rocks!', 'Amazing Project!', false, 1, 1, 1)
    ON CONFLICT DO NOTHING;


END;
$$ LANGUAGE plpgsql;
