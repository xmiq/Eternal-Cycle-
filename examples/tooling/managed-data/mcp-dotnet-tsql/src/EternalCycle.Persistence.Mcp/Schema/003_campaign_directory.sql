SET XACT_ABORT ON;
GO

-- Additive upgrade for reference deployments created before managed campaign
-- discovery exposed human-readable names. Safe to execute repeatedly.
IF COL_LENGTH(N'ec.campaigns', N'display_name') IS NULL
    ALTER TABLE ec.campaigns ADD display_name nvarchar(256) NULL;
GO

IF COL_LENGTH(N'ec.campaigns', N'description') IS NULL
    ALTER TABLE ec.campaigns ADD description nvarchar(1000) NULL;
GO
