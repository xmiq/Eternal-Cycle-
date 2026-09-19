SET XACT_ABORT ON;
GO

-- Bounded execution ownership distinguishes live worker claims from orphaned
-- Running rows after process or host loss. Existing Running rows intentionally
-- receive NULL ownership and are recovered as Interrupted by the worker.
IF COL_LENGTH(N'ec_domain.managed_operations', N'execution_owner_id') IS NULL
    ALTER TABLE [ec_domain].managed_operations ADD execution_owner_id nvarchar(128) NULL;
GO

IF COL_LENGTH(N'ec_domain.managed_operations', N'execution_lease_expires_at') IS NULL
    ALTER TABLE [ec_domain].managed_operations ADD execution_lease_expires_at datetimeoffset(7) NULL;
GO

IF COL_LENGTH(N'ec_domain.managed_operations', N'execution_attempt_count') IS NULL
    ALTER TABLE [ec_domain].managed_operations ADD execution_attempt_count int NOT NULL
        CONSTRAINT DF_ec_domain_managed_operations_attempt_count DEFAULT (0);
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID(N'[ec_domain].managed_operations')
      AND name = N'CK_ec_domain_managed_operations_execution_ownership'
)
    ALTER TABLE [ec_domain].managed_operations ADD
        CONSTRAINT CK_ec_domain_managed_operations_execution_ownership CHECK (
            (execution_owner_id IS NULL AND execution_lease_expires_at IS NULL)
            OR
            (execution_owner_id IS NOT NULL AND execution_lease_expires_at IS NOT NULL)
        );
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID(N'[ec_domain].managed_operations')
      AND name = N'CK_ec_domain_managed_operations_attempt_count'
)
    ALTER TABLE [ec_domain].managed_operations ADD
        CONSTRAINT CK_ec_domain_managed_operations_attempt_count CHECK (execution_attempt_count >= 0);
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'[ec_domain].managed_operations')
      AND name = N'IX_ec_domain_managed_operations_execution_lease'
)
    CREATE INDEX IX_ec_domain_managed_operations_execution_lease
        ON [ec_domain].managed_operations (
            operation_state,
            execution_lease_expires_at,
            operation_id
        );
GO
