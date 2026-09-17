SET XACT_ABORT ON;
GO

-- Example default-domain form. Deployments with another trusted Domain
-- Namespace render 005_managed_operation_diagnostics.template.sql instead.
IF OBJECT_ID(N'ec_domain.managed_operation_diagnostics', N'U') IS NULL
BEGIN
    CREATE TABLE ec_domain.managed_operation_diagnostics (
        diagnostic_id nvarchar(128) NOT NULL,
        correlation_id nvarchar(128) NOT NULL,
        recorded_at datetimeoffset(7) NOT NULL,
        operation_name nvarchar(128) NOT NULL,
        operation_stage nvarchar(64) NOT NULL,
        error_code nvarchar(128) NOT NULL,
        exception_type nvarchar(512) NOT NULL,
        sanitized_exception_message nvarchar(2000) NOT NULL,
        sanitized_inner_exception_chain nvarchar(max) NULL,
        sanitized_stack_trace nvarchar(max) NULL,
        eternal_cycle_version nvarchar(64) NOT NULL,
        implementation_version nvarchar(64) NOT NULL,
        ruleset_id nvarchar(128) NULL,
        rule_release_id nvarchar(128) NULL,
        source_identity nvarchar(256) NULL,
        campaign_id nvarchar(128) NULL,
        duration_ms bigint NOT NULL,
        outcome nvarchar(32) NOT NULL,
        CONSTRAINT PK_ec_domain_managed_operation_diagnostics PRIMARY KEY (diagnostic_id),
        CONSTRAINT CK_ec_domain_managed_operation_diagnostics_duration CHECK (duration_ms >= 0)
    );

    CREATE INDEX IX_ec_domain_managed_operation_diagnostics_correlation
        ON ec_domain.managed_operation_diagnostics (correlation_id, recorded_at DESC);

    CREATE INDEX IX_ec_domain_managed_operation_diagnostics_operation
        ON ec_domain.managed_operation_diagnostics (operation_name, recorded_at DESC);
END;
GO
