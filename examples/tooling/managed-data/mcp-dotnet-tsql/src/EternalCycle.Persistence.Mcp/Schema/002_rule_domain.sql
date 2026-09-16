SET XACT_ABORT ON;
GO

-- Reference Domain Namespace for published Derived rules. Canonical Markdown
-- remains authoritative. World-specific Campaign Canon never belongs here.
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'ec_domain')
    EXEC(N'CREATE SCHEMA [ec_domain] AUTHORIZATION dbo;');
GO

CREATE TABLE [ec_domain].rulesets (
    ruleset_id nvarchar(128) NOT NULL,
    display_name nvarchar(256) NOT NULL,
    created_at datetimeoffset(7) NOT NULL CONSTRAINT DF_ec_domain_rulesets_created DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ec_domain_rulesets PRIMARY KEY (ruleset_id)
);
GO

CREATE TABLE [ec_domain].rule_releases (
    rule_release_id nvarchar(128) NOT NULL,
    ruleset_id nvarchar(128) NOT NULL,
    provider_kind nvarchar(64) NOT NULL,
    source_identity nvarchar(256) NOT NULL,
    repository_version nvarchar(128) NOT NULL,
    compiler_version nvarchar(64) NOT NULL,
    release_state nvarchar(32) NOT NULL,
    compiled_index_json nvarchar(max) NOT NULL,
    failure_reason nvarchar(500) NULL,
    created_at datetimeoffset(7) NOT NULL,
    validated_at datetimeoffset(7) NULL,
    published_at datetimeoffset(7) NULL,
    activated_at datetimeoffset(7) NULL,
    CONSTRAINT PK_ec_domain_rule_releases PRIMARY KEY (rule_release_id),
    CONSTRAINT FK_ec_domain_rule_releases_rulesets FOREIGN KEY (ruleset_id)
        REFERENCES [ec_domain].rulesets(ruleset_id),
    CONSTRAINT UQ_ec_domain_rule_releases_source UNIQUE (ruleset_id, source_identity),
    CONSTRAINT CK_ec_domain_rule_releases_state CHECK (release_state IN (
        N'Candidate', N'Validated', N'Published', N'Active', N'Failed'
    )),
    CONSTRAINT CK_ec_domain_rule_releases_json CHECK (ISJSON(compiled_index_json) = 1)
);
GO

CREATE TABLE [ec_domain].rule_chunks (
    rule_release_id nvarchar(128) NOT NULL,
    chunk_id nvarchar(256) NOT NULL,
    rule_source_id nvarchar(128) NOT NULL,
    source_path nvarchar(1000) NOT NULL,
    source_anchor nvarchar(512) NULL,
    source_hash char(64) NOT NULL,
    rule_layer nvarchar(32) NOT NULL,
    priority int NOT NULL,
    always_include bit NOT NULL,
    estimated_tokens int NOT NULL,
    content nvarchar(max) NOT NULL,
    metadata_json nvarchar(max) NOT NULL,
    CONSTRAINT PK_ec_domain_rule_chunks PRIMARY KEY (rule_release_id, chunk_id),
    CONSTRAINT FK_ec_domain_rule_chunks_release FOREIGN KEY (rule_release_id)
        REFERENCES [ec_domain].rule_releases(rule_release_id),
    CONSTRAINT CK_ec_domain_rule_chunks_tokens CHECK (estimated_tokens > 0),
    CONSTRAINT CK_ec_domain_rule_chunks_metadata CHECK (ISJSON(metadata_json) = 1)
);
GO

CREATE TABLE [ec_domain].rule_chunk_selectors (
    rule_release_id nvarchar(128) NOT NULL,
    chunk_id nvarchar(256) NOT NULL,
    selector_type nvarchar(32) NOT NULL,
    selector_value nvarchar(256) NOT NULL,
    CONSTRAINT PK_ec_domain_rule_chunk_selectors PRIMARY KEY (
        rule_release_id, chunk_id, selector_type, selector_value
    ),
    CONSTRAINT FK_ec_domain_rule_chunk_selectors_chunk FOREIGN KEY (rule_release_id, chunk_id)
        REFERENCES [ec_domain].rule_chunks(rule_release_id, chunk_id),
    CONSTRAINT CK_ec_domain_rule_chunk_selectors_type CHECK (selector_type IN (
        N'WorldModel', N'Module', N'CampaignMode', N'Operation', N'Topic'
    ))
);
GO

CREATE INDEX IX_ec_domain_rule_chunk_selector_lookup
    ON [ec_domain].rule_chunk_selectors (rule_release_id, selector_type, selector_value, chunk_id);
GO

CREATE TABLE [ec_domain].rule_dependencies (
    rule_release_id nvarchar(128) NOT NULL,
    source_chunk_id nvarchar(256) NOT NULL,
    required_chunk_id nvarchar(256) NOT NULL,
    dependency_reason nvarchar(256) NOT NULL,
    CONSTRAINT PK_ec_domain_rule_dependencies PRIMARY KEY (
        rule_release_id, source_chunk_id, required_chunk_id
    ),
    CONSTRAINT FK_ec_domain_rule_dependencies_source FOREIGN KEY (rule_release_id, source_chunk_id)
        REFERENCES [ec_domain].rule_chunks(rule_release_id, chunk_id),
    CONSTRAINT FK_ec_domain_rule_dependencies_target FOREIGN KEY (rule_release_id, required_chunk_id)
        REFERENCES [ec_domain].rule_chunks(rule_release_id, chunk_id)
);
GO

CREATE TABLE [ec_domain].active_rule_releases (
    ruleset_id nvarchar(128) NOT NULL,
    rule_release_id nvarchar(128) NOT NULL,
    activated_at datetimeoffset(7) NOT NULL,
    CONSTRAINT PK_ec_domain_active_rule_releases PRIMARY KEY (ruleset_id),
    CONSTRAINT FK_ec_domain_active_rule_releases_ruleset FOREIGN KEY (ruleset_id)
        REFERENCES [ec_domain].rulesets(ruleset_id),
    CONSTRAINT FK_ec_domain_active_rule_releases_release FOREIGN KEY (rule_release_id)
        REFERENCES [ec_domain].rule_releases(rule_release_id)
);
GO

CREATE TABLE [ec_domain].rule_update_checks (
    update_check_id nvarchar(128) NOT NULL,
    ruleset_id nvarchar(128) NOT NULL,
    source_identity nvarchar(256) NULL,
    outcome nvarchar(32) NOT NULL,
    sanitized_detail nvarchar(500) NULL,
    checked_at datetimeoffset(7) NOT NULL,
    CONSTRAINT PK_ec_domain_rule_update_checks PRIMARY KEY (update_check_id),
    CONSTRAINT FK_ec_domain_rule_update_checks_ruleset FOREIGN KEY (ruleset_id)
        REFERENCES [ec_domain].rulesets(ruleset_id),
    CONSTRAINT CK_ec_domain_rule_update_checks_outcome CHECK (outcome IN (
        N'Unchanged', N'Candidate', N'Activated', N'Degraded', N'Failed'
    ))
);
GO
