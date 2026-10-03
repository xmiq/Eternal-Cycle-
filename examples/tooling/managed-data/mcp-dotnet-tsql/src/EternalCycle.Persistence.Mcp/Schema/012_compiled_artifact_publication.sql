SET XACT_ABORT ON;
GO

-- Publication/activation are separate from import. Legacy release and campaign
-- bindings remain untouched. Composite FKs enforce Ruleset ownership.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'ec_domain.imported_rule_artifacts')
    AND name = N'UX_imported_artifacts_ruleset_id')
    CREATE UNIQUE INDEX UX_imported_artifacts_ruleset_id
        ON [ec_domain].imported_rule_artifacts (ruleset_id, import_id);
GO

IF OBJECT_ID(N'ec_domain.published_rule_artifacts', N'U') IS NULL
BEGIN
    CREATE TABLE [ec_domain].published_rule_artifacts (
        ruleset_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        import_id varchar(73) COLLATE Latin1_General_100_BIN2 NOT NULL,
        published_at datetimeoffset(7) NOT NULL DEFAULT (SYSUTCDATETIME()),
        PRIMARY KEY (ruleset_id, import_id),
        FOREIGN KEY (ruleset_id, import_id)
            REFERENCES [ec_domain].imported_rule_artifacts (ruleset_id, import_id)
    );
END;
GO

IF OBJECT_ID(N'ec_domain.active_rule_artifacts', N'U') IS NULL
BEGIN
    CREATE TABLE [ec_domain].active_rule_artifacts (
        ruleset_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL PRIMARY KEY,
        import_id varchar(73) COLLATE Latin1_General_100_BIN2 NOT NULL,
        FOREIGN KEY (ruleset_id, import_id)
            REFERENCES [ec_domain].published_rule_artifacts (ruleset_id, import_id)
    );
END;
GO
