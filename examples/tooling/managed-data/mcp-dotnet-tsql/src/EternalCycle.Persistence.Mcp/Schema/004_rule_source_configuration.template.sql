SET XACT_ABORT ON;
GO

-- Render only through SqlServerSchemaMigration.RenderDomain. Source
-- credentials never belong in this table. Safe to execute repeatedly.
IF OBJECT_ID(N'{{schema}}.rule_source_configurations', N'U') IS NULL
BEGIN
    CREATE TABLE {{schema}}.rule_source_configurations (
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
