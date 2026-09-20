SET XACT_ABORT ON;
GO

-- The generic Managed service records setup progress without claiming that a
-- user confirmation is a technical host attestation.
IF OBJECT_ID(N'{{schema_name}}.gm_host_configurations', N'U') IS NULL
BEGIN
    CREATE TABLE {{schema}}.gm_host_configurations (
        ruleset_id nvarchar(128) NOT NULL,
        bootstrap_source_hash char(64) NOT NULL,
        configuration_state nvarchar(32) NOT NULL,
        presented_at datetimeoffset(7) NULL,
        confirmed_at datetimeoffset(7) NULL,
        verified_at datetimeoffset(7) NULL,
        configuration_revision bigint NOT NULL,
        updated_at datetimeoffset(7) NOT NULL,
        CONSTRAINT PK_ec_domain_gm_host_configurations PRIMARY KEY (ruleset_id),
        CONSTRAINT FK_ec_domain_gm_host_configurations_ruleset FOREIGN KEY (ruleset_id)
            REFERENCES {{schema}}.rulesets(ruleset_id),
        CONSTRAINT CK_ec_domain_gm_host_configurations_hash CHECK (
            bootstrap_source_hash NOT LIKE '%[^0-9A-F]%'
            AND LEN(bootstrap_source_hash) = 64
        ),
        CONSTRAINT CK_ec_domain_gm_host_configurations_state CHECK (
            configuration_state IN (
                N'Required', N'InstructionsPresented', N'UserConfirmed', N'Verified'
            )
        ),
        CONSTRAINT CK_ec_domain_gm_host_configurations_revision CHECK (
            configuration_revision > 0
        )
    );
END;
GO
