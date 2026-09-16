SET XACT_ABORT ON;
GO

-- Durable administrative configuration for the reference Rule Source
-- Provider. Source credentials never belong in this table.
IF OBJECT_ID(N'[ec_domain].rule_source_configurations', N'U') IS NULL
BEGIN
    CREATE TABLE [ec_domain].rule_source_configurations (
        ruleset_id nvarchar(128) NOT NULL,
        provider_kind nvarchar(64) NOT NULL,
        source_location nvarchar(2000) NOT NULL,
        requested_ref nvarchar(256) NOT NULL,
        manifest_path nvarchar(1000) NOT NULL,
        is_official bit NOT NULL,
        config_revision bigint NOT NULL CONSTRAINT DF_ec_domain_source_config_revision DEFAULT (1),
        configured_at datetimeoffset(7) NOT NULL,
        CONSTRAINT PK_ec_domain_rule_source_configurations PRIMARY KEY (ruleset_id),
        CONSTRAINT CK_ec_domain_source_config_revision CHECK (config_revision > 0)
    );
END;
GO
