SET XACT_ABORT ON;
GO

-- One bounded preparation/receipt per trusted interaction. No Canon or packets.
IF OBJECT_ID(N'ec_domain.gameplay_entries', N'U') IS NULL
BEGIN
    CREATE TABLE [ec_domain].gameplay_entries (
        scope_key char(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
        interaction_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        request_id nvarchar(128) COLLATE Latin1_General_100_BIN2 NOT NULL,
        request_hash char(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
        entry_json nvarchar(max) COLLATE Latin1_General_100_BIN2 NOT NULL,
        PRIMARY KEY (scope_key, interaction_id),
        UNIQUE (scope_key, request_id),
        FOREIGN KEY (scope_key, interaction_id) REFERENCES [ec_domain].player_interactions(scope_key, interaction_id),
        CHECK (ISJSON(entry_json) = 1 AND DATALENGTH(entry_json) <= 131072),
        CHECK (JSON_VALUE(entry_json, '$.RequestId') IS NOT NULL AND JSON_VALUE(entry_json, '$.RequestId') = request_id),
        CHECK (JSON_VALUE(entry_json, '$.Fingerprint') IS NOT NULL AND JSON_VALUE(entry_json, '$.Fingerprint') = request_hash),
        CHECK (TRY_CONVERT(bigint, JSON_VALUE(entry_json, '$.PendingRevision')) IS NOT NULL AND
            TRY_CONVERT(bigint, JSON_VALUE(entry_json, '$.PendingRevision')) > 0)
    );
END;
GO
