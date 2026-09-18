SET XACT_ABORT ON;
GO

-- Additive correlation support for durable Managed Operations. Existing
-- diagnostic evidence remains valid and may have a null operation_id.
IF COL_LENGTH(N'{{schema_name}}.managed_operation_diagnostics', N'operation_id') IS NULL
    ALTER TABLE {{schema}}.managed_operation_diagnostics ADD operation_id nvarchar(128) NULL;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'{{schema_name}}.managed_operation_diagnostics')
      AND name = N'IX_managed_operation_diagnostics_operation_id'
)
    CREATE INDEX IX_managed_operation_diagnostics_operation_id
        ON {{schema}}.managed_operation_diagnostics (operation_id, recorded_at DESC)
        WHERE operation_id IS NOT NULL;
GO
