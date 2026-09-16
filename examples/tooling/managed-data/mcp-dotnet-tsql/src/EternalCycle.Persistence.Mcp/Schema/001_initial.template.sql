SET XACT_ABORT ON;
GO

-- Render only through SqlServerSchemaMigration with a trusted, validated
-- world/schema binding. Never substitute player- or model-supplied text.
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'{{schema_name}}')
    EXEC(N'CREATE SCHEMA {{schema}} AUTHORIZATION dbo;');
GO

CREATE TABLE {{schema}}.campaigns (
    campaign_id nvarchar(128) NOT NULL,
    repository_version nvarchar(64) NOT NULL,
    persistence_model_version nvarchar(64) NOT NULL,
    active_version bigint NOT NULL CONSTRAINT DF_ec_campaigns_active_version DEFAULT (0),
    last_validated_commit_at datetimeoffset(7) NULL,
    created_at datetimeoffset(7) NOT NULL CONSTRAINT DF_ec_campaigns_created_at DEFAULT (SYSUTCDATETIME()),
    row_version rowversion NOT NULL,
    CONSTRAINT PK_ec_campaigns PRIMARY KEY (campaign_id),
    CONSTRAINT CK_ec_campaigns_active_version CHECK (active_version >= 0)
);
GO

CREATE TABLE {{schema}}.save_transactions (
    transaction_id nvarchar(128) NOT NULL,
    campaign_id nvarchar(128) NOT NULL,
    idempotency_key nvarchar(128) NOT NULL,
    request_hash char(64) NOT NULL,
    request_json nvarchar(max) NOT NULL,
    parent_version bigint NOT NULL,
    candidate_version bigint NOT NULL,
    source_interaction_id nvarchar(128) NOT NULL,
    affected_set_json nvarchar(max) NOT NULL,
    status nvarchar(32) NOT NULL,
    failure_reason nvarchar(2000) NULL,
    started_at datetimeoffset(7) NOT NULL,
    activated_at datetimeoffset(7) NULL,
    completed_at datetimeoffset(7) NULL,
    CONSTRAINT PK_ec_save_transactions PRIMARY KEY (transaction_id),
    CONSTRAINT FK_ec_save_transactions_campaigns FOREIGN KEY (campaign_id)
        REFERENCES {{schema}}.campaigns(campaign_id),
    CONSTRAINT UQ_ec_save_transactions_idempotency UNIQUE (campaign_id, idempotency_key),
    CONSTRAINT CK_ec_save_transactions_request_json CHECK (ISJSON(request_json) = 1),
    CONSTRAINT CK_ec_save_transactions_affected_set_json CHECK (ISJSON(affected_set_json) = 1),
    CONSTRAINT CK_ec_save_transactions_versions CHECK (
        parent_version >= 0 AND candidate_version = parent_version + 1
    ),
    CONSTRAINT CK_ec_save_transactions_status CHECK (status IN (
        N'Staged',
        N'CandidateValidated',
        N'ActivatedPendingReadback',
        N'Completed',
        N'FailedValidation',
        N'FailedActivation',
        N'FailedReadback',
        N'FailedDurability'
    ))
);
GO

CREATE TABLE {{schema}}.canonical_record_versions (
    campaign_id nvarchar(128) NOT NULL,
    owner_domain nvarchar(128) NOT NULL,
    record_id nvarchar(128) NOT NULL,
    campaign_version bigint NOT NULL,
    record_revision bigint NOT NULL,
    payload_json nvarchar(max) NOT NULL,
    payload_hash char(64) NOT NULL,
    is_tombstone bit NOT NULL,
    transaction_id nvarchar(128) NOT NULL,
    recorded_at datetimeoffset(7) NOT NULL,
    CONSTRAINT PK_ec_canonical_record_versions PRIMARY KEY (
        campaign_id,
        owner_domain,
        record_id,
        campaign_version
    ),
    CONSTRAINT FK_ec_record_versions_campaigns FOREIGN KEY (campaign_id)
        REFERENCES {{schema}}.campaigns(campaign_id),
    CONSTRAINT FK_ec_record_versions_transactions FOREIGN KEY (transaction_id)
        REFERENCES {{schema}}.save_transactions(transaction_id),
    CONSTRAINT CK_ec_record_versions_revision CHECK (record_revision > 0),
    CONSTRAINT CK_ec_record_versions_campaign_version CHECK (campaign_version > 0),
    CONSTRAINT CK_ec_record_versions_payload CHECK (ISJSON(payload_json) = 1)
);
GO

CREATE INDEX IX_ec_record_versions_effective
    ON {{schema}}.canonical_record_versions (
        campaign_id,
        owner_domain,
        record_id,
        campaign_version DESC
    )
    INCLUDE (record_revision, payload_hash, is_tombstone);
GO

CREATE TABLE {{schema}}.record_references (
    campaign_id nvarchar(128) NOT NULL,
    source_owner_domain nvarchar(128) NOT NULL,
    source_record_id nvarchar(128) NOT NULL,
    campaign_version bigint NOT NULL,
    relation_type nvarchar(128) NOT NULL,
    target_owner_domain nvarchar(128) NOT NULL,
    target_record_id nvarchar(128) NOT NULL,
    transaction_id nvarchar(128) NOT NULL,
    CONSTRAINT PK_ec_record_references PRIMARY KEY (
        campaign_id,
        source_owner_domain,
        source_record_id,
        campaign_version,
        relation_type,
        target_owner_domain,
        target_record_id
    ),
    CONSTRAINT FK_ec_record_references_source FOREIGN KEY (
        campaign_id,
        source_owner_domain,
        source_record_id,
        campaign_version
    ) REFERENCES {{schema}}.canonical_record_versions (
        campaign_id,
        owner_domain,
        record_id,
        campaign_version
    ),
    CONSTRAINT FK_ec_record_references_transactions FOREIGN KEY (transaction_id)
        REFERENCES {{schema}}.save_transactions(transaction_id)
);
GO

CREATE INDEX IX_ec_record_references_target
    ON {{schema}}.record_references (
        campaign_id,
        target_owner_domain,
        target_record_id,
        campaign_version DESC
    );
GO

CREATE TABLE {{schema}}.validation_runs (
    validation_id nvarchar(128) NOT NULL,
    campaign_id nvarchar(128) NOT NULL,
    transaction_id nvarchar(128) NOT NULL,
    campaign_version bigint NOT NULL,
    outcome nvarchar(16) NOT NULL,
    evidence nvarchar(2000) NOT NULL,
    completed_at datetimeoffset(7) NOT NULL,
    CONSTRAINT PK_ec_validation_runs PRIMARY KEY (validation_id),
    CONSTRAINT FK_ec_validation_runs_campaigns FOREIGN KEY (campaign_id)
        REFERENCES {{schema}}.campaigns(campaign_id),
    CONSTRAINT FK_ec_validation_runs_transactions FOREIGN KEY (transaction_id)
        REFERENCES {{schema}}.save_transactions(transaction_id),
    CONSTRAINT CK_ec_validation_runs_outcome CHECK (outcome IN (N'PASS', N'FAIL', N'WARNING'))
);
GO

CREATE TABLE {{schema}}.persistence_receipts (
    receipt_id nvarchar(128) NOT NULL,
    campaign_id nvarchar(128) NOT NULL,
    transaction_id nvarchar(128) NOT NULL,
    campaign_version bigint NOT NULL,
    status nvarchar(32) NOT NULL,
    validation_evidence nvarchar(2000) NOT NULL,
    completed_at datetimeoffset(7) NOT NULL,
    CONSTRAINT PK_ec_persistence_receipts PRIMARY KEY (receipt_id),
    CONSTRAINT UQ_ec_persistence_receipts_transaction UNIQUE (campaign_id, transaction_id),
    CONSTRAINT FK_ec_persistence_receipts_campaigns FOREIGN KEY (campaign_id)
        REFERENCES {{schema}}.campaigns(campaign_id),
    CONSTRAINT FK_ec_persistence_receipts_transactions FOREIGN KEY (transaction_id)
        REFERENCES {{schema}}.save_transactions(transaction_id),
    CONSTRAINT CK_ec_persistence_receipts_status CHECK (status IN (N'PendingReadback', N'Validated'))
);
GO

CREATE TABLE {{schema}}.recovery_points (
    recovery_point_id nvarchar(128) NOT NULL,
    campaign_id nvarchar(128) NOT NULL,
    campaign_version bigint NOT NULL,
    provider nvarchar(64) NOT NULL,
    provider_reference nvarchar(512) NOT NULL,
    content_hash char(64) NULL,
    status nvarchar(32) NOT NULL,
    created_at datetimeoffset(7) NOT NULL,
    verified_at datetimeoffset(7) NULL,
    CONSTRAINT PK_ec_recovery_points PRIMARY KEY (recovery_point_id),
    CONSTRAINT FK_ec_recovery_points_campaigns FOREIGN KEY (campaign_id)
        REFERENCES {{schema}}.campaigns(campaign_id),
    CONSTRAINT CK_ec_recovery_points_status CHECK (status IN (N'Pending', N'Verified', N'Failed'))
);
GO
