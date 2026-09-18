SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'ec_domain.managed_operation_diagnostics', N'operation_id') IS NULL
    ALTER TABLE ec_domain.managed_operation_diagnostics ADD operation_id nvarchar(128) NULL;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'ec_domain.managed_operation_diagnostics')
      AND name = N'IX_managed_operation_diagnostics_operation_id'
)
    CREATE INDEX IX_managed_operation_diagnostics_operation_id
        ON ec_domain.managed_operation_diagnostics (operation_id, recorded_at DESC)
        WHERE operation_id IS NOT NULL;
GO
