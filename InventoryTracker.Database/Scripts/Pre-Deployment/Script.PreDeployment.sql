/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed before the build script.	
 Use SQLCMD syntax to include a file in the pre-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the pre-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

-- Pre-deployment validation checks
PRINT 'Starting pre-deployment validation...';

-- Check SQL Server version compatibility
IF @@VERSION NOT LIKE '%Microsoft SQL Server%'
BEGIN
    RAISERROR('This database requires Microsoft SQL Server', 16, 1);
    RETURN;
END

-- Check for sufficient permissions
IF IS_SRVROLEMEMBER('dbcreator') = 0 AND IS_SRVROLEMEMBER('sysadmin') = 0
BEGIN
    RAISERROR('Insufficient permissions for database deployment. User must be a member of dbcreator or sysadmin role.', 16, 1);
    RETURN;
END

-- Backup existing data if tables exist (for upgrade scenarios)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'RFID' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Existing RFID table detected. Consider backing up data before deployment.';
    
    -- Create backup tables if they don't exist
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RFID_Backup' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        PRINT 'Creating backup of existing RFID data...';
        SELECT * INTO dbo.RFID_Backup FROM dbo.RFID;
        PRINT 'RFID data backed up to RFID_Backup table.';
    END
END

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'List' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Existing List table detected. Consider backing up data before deployment.';
    
    -- Create backup tables if they don't exist
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'List_Backup' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        PRINT 'Creating backup of existing List data...';
        SELECT * INTO dbo.List_Backup FROM dbo.List;
        PRINT 'List data backed up to List_Backup table.';
    END
END

-- Set deployment variables
:setvar DeploymentDate "GETDATE()"
:setvar DeploymentUser "SYSTEM_USER"

PRINT 'Pre-deployment validation completed successfully.';
PRINT 'Deployment Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT 'Deployment User: ' + SYSTEM_USER;
