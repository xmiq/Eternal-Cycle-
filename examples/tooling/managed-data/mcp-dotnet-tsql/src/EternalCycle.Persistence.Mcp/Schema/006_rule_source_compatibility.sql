SET XACT_ABORT ON;
GO

-- Default ec_domain form of the additive rule-source compatibility migration.
IF COL_LENGTH(N'ec_domain.rule_source_configurations', N'release_channel') IS NULL
    ALTER TABLE ec_domain.rule_source_configurations
        ADD release_channel nvarchar(32) NOT NULL
            CONSTRAINT DF_ec_domain_rule_source_release_channel DEFAULT (N'Stable') WITH VALUES;
GO

IF COL_LENGTH(N'ec_domain.rule_releases', N'release_channel') IS NULL
    ALTER TABLE ec_domain.rule_releases
        ADD release_channel nvarchar(32) NOT NULL
            CONSTRAINT DF_ec_domain_rule_release_channel DEFAULT (N'Stable') WITH VALUES;
GO

IF COL_LENGTH(N'ec_domain.rule_releases', N'discovery_ref') IS NULL
    ALTER TABLE ec_domain.rule_releases ADD discovery_ref nvarchar(256) NULL;
GO

IF COL_LENGTH(N'ec_domain.rule_releases', N'manifest_format_version') IS NULL
    ALTER TABLE ec_domain.rule_releases ADD manifest_format_version int NULL;
GO

IF COL_LENGTH(N'ec_domain.rule_releases', N'compiler_contract_version') IS NULL
    ALTER TABLE ec_domain.rule_releases ADD compiler_contract_version nvarchar(64) NULL;
GO

IF COL_LENGTH(N'ec_domain.managed_operation_diagnostics', N'safe_detail') IS NULL
    ALTER TABLE ec_domain.managed_operation_diagnostics ADD safe_detail nvarchar(2000) NULL;
GO

IF COL_LENGTH(N'ec_domain.managed_operation_diagnostics', N'retry_safe') IS NULL
    ALTER TABLE ec_domain.managed_operation_diagnostics ADD retry_safe bit NULL;
GO

IF COL_LENGTH(N'ec_domain.managed_operation_diagnostics', N'administrative_intervention_required') IS NULL
    ALTER TABLE ec_domain.managed_operation_diagnostics ADD administrative_intervention_required bit NULL;
GO

IF COL_LENGTH(N'ec_domain.managed_operation_diagnostics', N'source_channel') IS NULL
    ALTER TABLE ec_domain.managed_operation_diagnostics ADD source_channel nvarchar(32) NULL;
GO

IF COL_LENGTH(N'ec_domain.managed_operation_diagnostics', N'discovery_ref') IS NULL
    ALTER TABLE ec_domain.managed_operation_diagnostics ADD discovery_ref nvarchar(256) NULL;
GO
