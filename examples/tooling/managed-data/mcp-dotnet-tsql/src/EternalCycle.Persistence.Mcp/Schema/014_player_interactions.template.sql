SET XACT_ABORT ON;
GO

-- Additive, opt-in control-plane records. Never infer submissions from old
-- transactions, conversation, campaign records or existing bindings.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'{{schema_name}}.campaign_session_bindings') AND name = N'UX_binding_interaction_owner')
    CREATE UNIQUE INDEX UX_binding_interaction_owner ON {{schema}}.campaign_session_bindings(scope_key, binding_id, generation, campaign_id);
GO

IF OBJECT_ID(N'{{schema_name}}.player_interactions', N'U') IS NULL
BEGIN
    CREATE TABLE {{schema}}.player_interactions (
        sequence_number bigint IDENTITY NOT NULL,
        scope_key char(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
        interaction_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        submission_hash char(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
        submission_fingerprint char(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
        binding_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        binding_generation bigint NOT NULL,
        campaign_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        interaction_state nvarchar(32) COLLATE Latin1_General_100_BIN2 NOT NULL CHECK (interaction_state IN
            (N'RECEIVED', N'ENTRY_PENDING', N'OPEN', N'PERSISTING', N'AWAITING_PLAYER_INPUT', N'COMPLETED', N'BLOCKED', N'CANCELLED')),
        revision bigint NOT NULL CHECK (revision > 0),
        persistence_unknown bit NOT NULL,
        interaction_json nvarchar(max) COLLATE Latin1_General_100_BIN2 NOT NULL CHECK (ISJSON(interaction_json) = 1),
        PRIMARY KEY (scope_key, interaction_id),
        UNIQUE (scope_key, submission_hash),
        UNIQUE (scope_key, interaction_id, binding_id, campaign_id),
        FOREIGN KEY (scope_key, binding_id, binding_generation, campaign_id)
            REFERENCES {{schema}}.campaign_session_bindings(scope_key, binding_id, generation, campaign_id),
        CHECK (JSON_VALUE(interaction_json, '$.InteractionId') IS NOT NULL AND JSON_VALUE(interaction_json, '$.InteractionId') = interaction_id),
        CHECK (JSON_VALUE(interaction_json, '$.ScopeKey') IS NOT NULL AND JSON_VALUE(interaction_json, '$.ScopeKey') = scope_key),
        CHECK (JSON_VALUE(interaction_json, '$.SubmissionHash') IS NOT NULL AND JSON_VALUE(interaction_json, '$.SubmissionHash') = submission_hash),
        CHECK (JSON_VALUE(interaction_json, '$.SubmissionFingerprint') IS NOT NULL AND JSON_VALUE(interaction_json, '$.SubmissionFingerprint') = submission_fingerprint),
        CHECK (JSON_VALUE(interaction_json, '$.BindingId') IS NOT NULL AND JSON_VALUE(interaction_json, '$.BindingId') = binding_id),
        CHECK (JSON_VALUE(interaction_json, '$.CampaignId') IS NOT NULL AND JSON_VALUE(interaction_json, '$.CampaignId') = campaign_id),
        CHECK (TRY_CONVERT(bigint, JSON_VALUE(interaction_json, '$.BindingGeneration')) IS NOT NULL AND TRY_CONVERT(bigint, JSON_VALUE(interaction_json, '$.BindingGeneration')) = binding_generation),
        CHECK (TRY_CONVERT(bigint, JSON_VALUE(interaction_json, '$.Revision')) IS NOT NULL AND TRY_CONVERT(bigint, JSON_VALUE(interaction_json, '$.Revision')) = revision),
        CHECK (JSON_VALUE(interaction_json, '$.State') IS NOT NULL AND JSON_VALUE(interaction_json, '$.State') = interaction_state),
        CHECK (JSON_VALUE(interaction_json, '$.PersistenceUnknown') IS NOT NULL AND
            JSON_VALUE(interaction_json, '$.PersistenceUnknown') = CASE WHEN persistence_unknown = 1 THEN N'true' ELSE N'false' END)
    );
    CREATE UNIQUE INDEX UX_player_interaction_forward ON {{schema}}.player_interactions(scope_key, binding_id)
        WHERE interaction_state IN (N'RECEIVED', N'ENTRY_PENDING', N'OPEN', N'PERSISTING', N'BLOCKED');
END;
GO

IF OBJECT_ID(N'{{schema_name}}.player_pending_decisions', N'U') IS NULL
BEGIN
    CREATE TABLE {{schema}}.player_pending_decisions (
        scope_key char(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
        decision_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        binding_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        campaign_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        origin_interaction_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        related_interaction_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NULL,
        decision_status nvarchar(16) COLLATE Latin1_General_100_BIN2 NOT NULL CHECK (decision_status IN (N'Pending', N'Resolved', N'Abandoned')),
        revision bigint NOT NULL CHECK (revision > 0),
        decision_json nvarchar(max) COLLATE Latin1_General_100_BIN2 NOT NULL CHECK (ISJSON(decision_json) = 1),
        PRIMARY KEY (scope_key, decision_id),
        FOREIGN KEY (scope_key, origin_interaction_id, binding_id, campaign_id)
            REFERENCES {{schema}}.player_interactions(scope_key, interaction_id, binding_id, campaign_id),
        FOREIGN KEY (scope_key, related_interaction_id, binding_id, campaign_id)
            REFERENCES {{schema}}.player_interactions(scope_key, interaction_id, binding_id, campaign_id),
        CHECK (JSON_VALUE(decision_json, '$.DecisionId') IS NOT NULL AND JSON_VALUE(decision_json, '$.DecisionId') = decision_id),
        CHECK (JSON_VALUE(decision_json, '$.ScopeKey') IS NOT NULL AND JSON_VALUE(decision_json, '$.ScopeKey') = scope_key),
        CHECK (JSON_VALUE(decision_json, '$.BindingId') IS NOT NULL AND JSON_VALUE(decision_json, '$.BindingId') = binding_id),
        CHECK (JSON_VALUE(decision_json, '$.CampaignId') IS NOT NULL AND JSON_VALUE(decision_json, '$.CampaignId') = campaign_id),
        CHECK (JSON_VALUE(decision_json, '$.OriginInteractionId') IS NOT NULL AND JSON_VALUE(decision_json, '$.OriginInteractionId') = origin_interaction_id),
        CHECK (JSON_VALUE(decision_json, '$.State') IS NOT NULL AND JSON_VALUE(decision_json, '$.State') = decision_status),
        CHECK (TRY_CONVERT(bigint, JSON_VALUE(decision_json, '$.Revision')) IS NOT NULL AND TRY_CONVERT(bigint, JSON_VALUE(decision_json, '$.Revision')) = revision),
        CHECK (ISNULL(JSON_VALUE(decision_json, '$.RelatedInteractionId'), N'') = ISNULL(related_interaction_id, N''))
    );
    CREATE UNIQUE INDEX UX_player_decision_pending ON {{schema}}.player_pending_decisions(scope_key, binding_id) WHERE decision_status = N'Pending';
END;
GO

IF OBJECT_ID(N'{{schema_name}}.player_interaction_receipts', N'U') IS NULL
BEGIN
    CREATE TABLE {{schema}}.player_interaction_receipts (
        scope_key char(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
        request_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        request_hash char(64) NOT NULL,
        interaction_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        transition_owner nvarchar(16) NOT NULL CHECK (transition_owner IN (N'Entry', N'Persistence', N'Yield', N'Recovery')),
        previous_state nvarchar(32) NOT NULL,
        resulting_state nvarchar(32) NOT NULL,
        resulting_revision bigint NOT NULL CHECK (resulting_revision > 0),
        PRIMARY KEY (scope_key, request_id),
        FOREIGN KEY (scope_key, interaction_id) REFERENCES {{schema}}.player_interactions(scope_key, interaction_id)
    );
END;
GO
