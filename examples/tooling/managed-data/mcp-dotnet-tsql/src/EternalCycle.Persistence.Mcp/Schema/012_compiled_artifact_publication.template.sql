SET XACT_ABORT ON;
GO

-- Publication/activation are separate from import. Legacy release and campaign
-- bindings remain untouched. Composite FKs enforce Ruleset ownership.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'{{schema_name}}.imported_rule_artifacts')
    AND name = N'UX_imported_artifacts_ruleset_id')
    CREATE UNIQUE INDEX UX_imported_artifacts_ruleset_id
        ON {{schema}}.imported_rule_artifacts (ruleset_id, import_id);
GO

IF OBJECT_ID(N'{{schema_name}}.published_rule_artifacts', N'U') IS NULL
BEGIN
    CREATE TABLE {{schema}}.published_rule_artifacts (
        ruleset_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        import_id varchar(73) COLLATE Latin1_General_100_BIN2 NOT NULL,
        published_at datetimeoffset(7) NOT NULL DEFAULT (SYSUTCDATETIME()),
        PRIMARY KEY (ruleset_id, import_id),
        FOREIGN KEY (ruleset_id, import_id)
            REFERENCES {{schema}}.imported_rule_artifacts (ruleset_id, import_id)
    );
END;
GO

IF OBJECT_ID(N'{{schema_name}}.active_rule_artifacts', N'U') IS NULL
BEGIN
    CREATE TABLE {{schema}}.active_rule_artifacts (
        ruleset_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL PRIMARY KEY,
        import_id varchar(73) COLLATE Latin1_General_100_BIN2 NOT NULL,
        FOREIGN KEY (ruleset_id, import_id)
            REFERENCES {{schema}}.published_rule_artifacts (ruleset_id, import_id)
    );
END;
GO
