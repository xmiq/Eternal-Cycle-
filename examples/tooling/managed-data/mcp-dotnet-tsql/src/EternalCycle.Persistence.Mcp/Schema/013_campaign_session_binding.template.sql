SET XACT_ABORT ON;
GO

-- Opt-in control-plane storage only. No campaign, save, rule release or
-- historical transaction is transformed, and no binding is guessed on upgrade.
IF OBJECT_ID(N'{{schema_name}}.campaign_session_bindings', N'U') IS NULL
BEGIN
    CREATE TABLE {{schema}}.campaign_session_bindings (
        scope_key char(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
        binding_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        generation bigint NOT NULL CHECK (generation > 0),
        campaign_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        binding_status nvarchar(16) COLLATE Latin1_General_100_BIN2 NOT NULL CHECK (binding_status IN (N'ACTIVE', N'SUSPENDED', N'CLOSED')),
        revision bigint NOT NULL CHECK (revision > 0),
        predecessor_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NULL,
        successor_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NULL,
        binding_json nvarchar(max) COLLATE Latin1_General_100_BIN2 NOT NULL CHECK (ISJSON(binding_json) = 1),
        PRIMARY KEY (scope_key, binding_id),
        UNIQUE (scope_key, generation),
        FOREIGN KEY (scope_key, predecessor_id) REFERENCES {{schema}}.campaign_session_bindings(scope_key, binding_id),
        FOREIGN KEY (scope_key, successor_id) REFERENCES {{schema}}.campaign_session_bindings(scope_key, binding_id),
        CHECK (JSON_VALUE(binding_json, '$.BindingId') IS NOT NULL AND JSON_VALUE(binding_json, '$.BindingId') = binding_id),
        CHECK (JSON_VALUE(binding_json, '$.ScopeKey') IS NOT NULL AND JSON_VALUE(binding_json, '$.ScopeKey') = scope_key),
        CHECK (JSON_VALUE(binding_json, '$.CampaignId') IS NOT NULL AND JSON_VALUE(binding_json, '$.CampaignId') = campaign_id),
        CHECK (JSON_VALUE(binding_json, '$.State') IS NOT NULL AND JSON_VALUE(binding_json, '$.State') = binding_status),
        CHECK (JSON_VALUE(binding_json, '$.Revision') IS NOT NULL AND TRY_CONVERT(bigint, JSON_VALUE(binding_json, '$.Revision')) = revision),
        CHECK (JSON_VALUE(binding_json, '$.Generation') IS NOT NULL AND TRY_CONVERT(bigint, JSON_VALUE(binding_json, '$.Generation')) = generation)
    );
    -- A suspended binding still owns the current session; discovery cannot
    -- substitute another campaign while this row exists.
    CREATE UNIQUE INDEX UX_campaign_binding_current_scope
        ON {{schema}}.campaign_session_bindings(scope_key) WHERE binding_status <> N'CLOSED';
END;
GO

IF OBJECT_ID(N'{{schema_name}}.campaign_binding_receipts', N'U') IS NULL
BEGIN
    CREATE TABLE {{schema}}.campaign_binding_receipts (
        scope_key char(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
        request_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        request_hash char(64) NOT NULL,
        binding_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        correlation_id nvarchar(128) NOT NULL,
        PRIMARY KEY (scope_key, request_id),
        FOREIGN KEY (scope_key, binding_id) REFERENCES {{schema}}.campaign_session_bindings(scope_key, binding_id)
    );
END;
GO
