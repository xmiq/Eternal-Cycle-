SET XACT_ABORT ON;
GO

-- Additive candidate storage: no legacy releases, chunks or active pointers are
-- rewritten. BIN2 avoids merging format-1 ordinal identities under DB collation.
IF OBJECT_ID(N'{{schema_name}}.imported_rule_artifacts', N'U') IS NULL
BEGIN
    CREATE TABLE {{schema}}.imported_rule_artifacts (
        import_id varchar(73) COLLATE Latin1_General_100_BIN2 NOT NULL PRIMARY KEY,
        ruleset_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        semantic_sha256 char(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
        byte_sha256 char(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
        exact_bytes varbinary(max) NOT NULL,
        header_json nvarchar(max) NOT NULL CHECK (ISJSON(header_json) = 1),
        imported_at datetimeoffset(7) NOT NULL DEFAULT (SYSUTCDATETIME()),
        UNIQUE (ruleset_id, semantic_sha256),
        CHECK (LEN(semantic_sha256) = 64 AND semantic_sha256 NOT LIKE '%[^0-9A-F]%'),
        CHECK (LEN(byte_sha256) = 64 AND byte_sha256 NOT LIKE '%[^0-9A-F]%'),
        CHECK (DATALENGTH(exact_bytes) > 0)
    );
END;
GO

IF OBJECT_ID(N'{{schema_name}}.imported_rule_sources', N'U') IS NULL
BEGIN
    CREATE TABLE {{schema}}.imported_rule_sources (
        import_id varchar(73) COLLATE Latin1_General_100_BIN2 NOT NULL,
        source_ordinal int NOT NULL CHECK (source_ordinal >= 0),
        rule_source_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        source_json nvarchar(max) NOT NULL CHECK (ISJSON(source_json) = 1),
        PRIMARY KEY (import_id, source_ordinal),
        UNIQUE (import_id, rule_source_id),
        FOREIGN KEY (import_id) REFERENCES {{schema}}.imported_rule_artifacts(import_id)
    );
END;
GO

IF OBJECT_ID(N'{{schema_name}}.imported_rule_snippets', N'U') IS NULL
BEGIN
    CREATE TABLE {{schema}}.imported_rule_snippets (
        import_id varchar(73) COLLATE Latin1_General_100_BIN2 NOT NULL,
        snippet_ordinal int NOT NULL CHECK (snippet_ordinal >= 0),
        source_ordinal int NOT NULL,
        snippet_json nvarchar(max) NOT NULL CHECK (ISJSON(snippet_json) = 1),
        PRIMARY KEY (import_id, snippet_ordinal),
        FOREIGN KEY (import_id, source_ordinal)
            REFERENCES {{schema}}.imported_rule_sources(import_id, source_ordinal)
    );
END;
GO

IF OBJECT_ID(N'{{schema_name}}.imported_rule_dependencies', N'U') IS NULL
BEGIN
    CREATE TABLE {{schema}}.imported_rule_dependencies (
        import_id varchar(73) COLLATE Latin1_General_100_BIN2 NOT NULL,
        source_ordinal int NOT NULL,
        dependency_ordinal int NOT NULL CHECK (dependency_ordinal >= 0),
        target_source_ordinal int NOT NULL,
        PRIMARY KEY (import_id, source_ordinal, dependency_ordinal),
        UNIQUE (import_id, source_ordinal, target_source_ordinal),
        CHECK (source_ordinal <> target_source_ordinal),
        FOREIGN KEY (import_id, source_ordinal)
            REFERENCES {{schema}}.imported_rule_sources(import_id, source_ordinal),
        FOREIGN KEY (import_id, target_source_ordinal)
            REFERENCES {{schema}}.imported_rule_sources(import_id, source_ordinal)
    );
END;
GO
