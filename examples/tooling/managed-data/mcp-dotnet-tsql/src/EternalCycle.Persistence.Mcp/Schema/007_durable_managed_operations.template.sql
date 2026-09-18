SET XACT_ABORT ON;
GO

-- Render this template with a validated Domain Namespace schema identifier.
IF OBJECT_ID(N'{{schema_name}}.managed_operations', N'U') IS NULL
BEGIN
    CREATE TABLE {{schema}}.managed_operations (
        operation_id nvarchar(128) NOT NULL,
        operation_kind nvarchar(128) NOT NULL,
        deduplication_key nvarchar(512) NOT NULL,
        correlation_id nvarchar(128) NOT NULL,
        operation_state nvarchar(32) NOT NULL,
        current_stage nvarchar(64) NOT NULL,
        created_at datetimeoffset(7) NOT NULL,
        started_at datetimeoffset(7) NULL,
        updated_at datetimeoffset(7) NOT NULL,
        completed_at datetimeoffset(7) NULL,
        progress_percent int NULL,
        safe_status_detail nvarchar(1000) NOT NULL,
        error_code nvarchar(128) NULL,
        retry_safe bit NOT NULL CONSTRAINT DF_ec_domain_managed_operations_retry DEFAULT (1),
        user_approval_required bit NOT NULL CONSTRAINT DF_ec_domain_managed_operations_approval DEFAULT (0),
        administrative_intervention_required bit NOT NULL CONSTRAINT DF_ec_domain_managed_operations_intervention DEFAULT (0),
        ruleset_id nvarchar(128) NULL,
        source_identity nvarchar(256) NULL,
        result_rule_release_id nvarchar(128) NULL,
        CONSTRAINT PK_ec_domain_managed_operations PRIMARY KEY (operation_id),
        CONSTRAINT FK_ec_domain_managed_operations_ruleset FOREIGN KEY (ruleset_id)
            REFERENCES {{schema}}.rulesets(ruleset_id),
        CONSTRAINT FK_ec_domain_managed_operations_release FOREIGN KEY (result_rule_release_id)
            REFERENCES {{schema}}.rule_releases(rule_release_id),
        CONSTRAINT CK_ec_domain_managed_operations_state CHECK (operation_state IN (
            N'Queued', N'Running', N'Succeeded', N'Failed',
            N'Cancelling', N'Cancelled', N'Interrupted'
        )),
        CONSTRAINT CK_ec_domain_managed_operations_progress CHECK (
            progress_percent IS NULL OR (progress_percent >= 0 AND progress_percent <= 100)
        )
    );

    CREATE INDEX IX_ec_domain_managed_operations_dispatch
        ON {{schema}}.managed_operations (operation_state, created_at, operation_id);

    CREATE INDEX IX_ec_domain_managed_operations_deduplication
        ON {{schema}}.managed_operations (deduplication_key, operation_state, updated_at DESC);

    CREATE INDEX IX_ec_domain_managed_operations_recent
        ON {{schema}}.managed_operations (operation_kind, updated_at DESC);
END;
GO

IF OBJECT_ID(N'{{schema_name}}.rule_source_preparation', N'U') IS NULL
BEGIN
    CREATE TABLE {{schema}}.rule_source_preparation (
        rule_release_id nvarchar(128) NOT NULL,
        rule_source_id nvarchar(128) NOT NULL,
        preparation_tier nvarchar(32) NOT NULL,
        preparation_state nvarchar(32) NOT NULL,
        base_priority int NOT NULL CONSTRAINT DF_ec_domain_rule_source_base_priority DEFAULT (0),
        priority_boost int NOT NULL CONSTRAINT DF_ec_domain_rule_source_priority_boost DEFAULT (0),
        failure_code nvarchar(128) NULL,
        updated_at datetimeoffset(7) NOT NULL,
        CONSTRAINT PK_ec_domain_rule_source_preparation PRIMARY KEY (rule_release_id, rule_source_id),
        CONSTRAINT FK_ec_domain_rule_source_preparation_release FOREIGN KEY (rule_release_id)
            REFERENCES {{schema}}.rule_releases(rule_release_id),
        CONSTRAINT CK_ec_domain_rule_source_preparation_tier CHECK (preparation_tier IN (
            N'RuntimeKernel', N'CampaignBootstrap', N'ImmediateGameplayCore',
            N'CampaignRelevant', N'Standard', N'OptionalRare'
        )),
        CONSTRAINT CK_ec_domain_rule_source_preparation_state CHECK (preparation_state IN (
            N'Pending', N'Ready', N'Failed'
        )),
        CONSTRAINT CK_ec_domain_rule_source_priority_boost CHECK (priority_boost >= 0)
    );

    CREATE INDEX IX_ec_domain_rule_source_preparation_dispatch
        ON {{schema}}.rule_source_preparation (
            rule_release_id, preparation_state, priority_boost DESC,
            preparation_tier, base_priority DESC, rule_source_id
        );
END;
GO

INSERT INTO {{schema}}.rule_source_preparation (
    rule_release_id, rule_source_id, preparation_tier,
    preparation_state, base_priority, priority_boost, updated_at
)
SELECT
    chunks.rule_release_id,
    chunks.rule_source_id,
    CASE
        WHEN MIN(CASE WHEN chunks.rule_layer = N'RuntimeKernel' THEN 0 ELSE 1 END) = 0
            THEN N'RuntimeKernel'
        ELSE N'Standard'
    END,
    N'Ready',
    MAX(chunks.priority),
    0,
    SYSUTCDATETIME()
FROM {{schema}}.rule_chunks AS chunks
WHERE NOT EXISTS (
    SELECT 1
    FROM {{schema}}.rule_source_preparation AS existing
    WHERE existing.rule_release_id = chunks.rule_release_id
      AND existing.rule_source_id = chunks.rule_source_id
)
GROUP BY chunks.rule_release_id, chunks.rule_source_id;
GO

IF COL_LENGTH(N'{{schema_name}}.rule_releases', N'base_release') IS NULL
    ALTER TABLE {{schema}}.rule_releases ADD base_release nvarchar(64) NULL;
GO

IF COL_LENGTH(N'{{schema_name}}.rule_releases', N'discovery_tag') IS NULL
    ALTER TABLE {{schema}}.rule_releases ADD discovery_tag nvarchar(128) NULL;
GO

IF COL_LENGTH(N'{{schema_name}}.rule_releases', N'commits_since_base') IS NULL
    ALTER TABLE {{schema}}.rule_releases ADD commits_since_base int NULL;
GO

IF COL_LENGTH(N'{{schema_name}}.rule_releases', N'display_version') IS NULL
    ALTER TABLE {{schema}}.rule_releases ADD display_version nvarchar(128) NULL;
GO
