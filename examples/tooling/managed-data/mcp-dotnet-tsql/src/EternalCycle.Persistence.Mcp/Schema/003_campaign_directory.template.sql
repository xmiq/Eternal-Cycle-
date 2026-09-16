SET XACT_ABORT ON;
GO

-- Render only through SqlServerSchemaMigration with a trusted, validated
-- world/schema binding. Safe to execute repeatedly.
IF COL_LENGTH(N'{{schema_name}}.campaigns', N'display_name') IS NULL
    ALTER TABLE {{schema}}.campaigns ADD display_name nvarchar(256) NULL;
GO

IF COL_LENGTH(N'{{schema_name}}.campaigns', N'description') IS NULL
    ALTER TABLE {{schema}}.campaigns ADD description nvarchar(1000) NULL;
GO
