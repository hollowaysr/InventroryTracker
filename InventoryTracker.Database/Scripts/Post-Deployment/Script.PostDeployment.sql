/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

PRINT 'Starting post-deployment configuration...';

-- Create sample data for development/testing (only if tables are empty)
IF NOT EXISTS (SELECT 1 FROM [dbo].[List])
BEGIN
    PRINT 'Inserting sample customer lists for development...';
    
    INSERT INTO [dbo].[List] ([Name], [Description], [SystemRef])
    VALUES 
        ('Electronics Inventory', 'RFID tags for electronic devices and components', 'SYS-ELEC-001'),
        ('Warehouse A - General', 'General inventory tracking for main warehouse', 'WH-A-GEN'),
        ('Asset Management', 'Company asset tracking and management', 'ASSET-MGMT-001'),
        ('Sample Products', 'Sample and demo products for testing', 'SAMPLE-001');
    
    PRINT 'Sample customer lists created.';
END

-- Insert sample RFID tags if lists exist but no tags exist
IF EXISTS (SELECT 1 FROM [dbo].[List]) AND NOT EXISTS (SELECT 1 FROM [dbo].[RFID])
BEGIN
    PRINT 'Inserting sample RFID tags for development...';
    
    DECLARE @ElectronicsListId INT = (SELECT TOP 1 Id FROM [dbo].[List] WHERE SystemRef = 'SYS-ELEC-001');
    DECLARE @WarehouseListId INT = (SELECT TOP 1 Id FROM [dbo].[List] WHERE SystemRef = 'WH-A-GEN');
    
    IF @ElectronicsListId IS NOT NULL
    BEGIN
        INSERT INTO [dbo].[RFID] ([RFID], [ListId], [Name], [Description], [Color], [Size])
        VALUES 
            ('RFID-001-LAPTOP', @ElectronicsListId, 'Dell Laptop #1', 'Dell Inspiron 15 - Serial: DL123456', 'Black', 'Large'),
            ('RFID-002-MOUSE', @ElectronicsListId, 'Wireless Mouse', 'Logitech MX Master 3', 'Blue', 'Small'),
            ('RFID-003-KEYBOARD', @ElectronicsListId, 'Mechanical Keyboard', 'Corsair K95 RGB Platinum', 'Black', 'Large'),
            ('RFID-004-MONITOR', @ElectronicsListId, 'Dell Monitor', '27-inch 4K Display - Model U2720Q', 'Silver', 'XL');
    END
    
    IF @WarehouseListId IS NOT NULL
    BEGIN
        INSERT INTO [dbo].[RFID] ([RFID], [ListId], [Name], [Description], [Color], [Size])
        VALUES 
            ('RFID-WH-001', @WarehouseListId, 'Storage Box A1', 'Archive documents storage box', 'Red', 'Medium'),
            ('RFID-WH-002', @WarehouseListId, 'Equipment Cart', 'Mobile equipment transportation cart', 'Green', 'Large'),
            ('RFID-WH-003', @WarehouseListId, 'Safety Kit', 'Emergency safety equipment kit', 'Yellow', 'Medium');
    END
    
    PRINT 'Sample RFID tags created.';
END

-- Create database maintenance jobs (if SQL Server Agent is available)
IF EXISTS (SELECT 1 FROM sys.dm_os_performance_counters WHERE object_name LIKE '%SQL Agent%')
BEGIN
    PRINT 'SQL Server Agent detected. Consider setting up maintenance jobs for:';
    PRINT '  - Index maintenance and statistics updates';
    PRINT '  - Data archival for old records';
    PRINT '  - Backup and recovery procedures';
END

-- Update database compatibility and settings
ALTER DATABASE CURRENT SET COMPATIBILITY_LEVEL = 150; -- SQL Server 2019
ALTER DATABASE CURRENT SET AUTO_CREATE_STATISTICS ON;
ALTER DATABASE CURRENT SET AUTO_UPDATE_STATISTICS ON;
ALTER DATABASE CURRENT SET AUTO_UPDATE_STATISTICS_ASYNC ON;

-- Enable Query Store for performance monitoring
IF SERVERPROPERTY('EngineEdition') != 5 -- Not SQL Database
BEGIN
    ALTER DATABASE CURRENT SET QUERY_STORE = ON;
    ALTER DATABASE CURRENT SET QUERY_STORE (
        OPERATION_MODE = READ_WRITE,
        CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30),
        DATA_FLUSH_INTERVAL_SECONDS = 900,
        INTERVAL_LENGTH_MINUTES = 60,
        MAX_STORAGE_SIZE_MB = 1000
    );
    PRINT 'Query Store enabled for performance monitoring.';
END

-- Final validation
DECLARE @ListCount INT = (SELECT COUNT(*) FROM [dbo].[List]);
DECLARE @RfidCount INT = (SELECT COUNT(*) FROM [dbo].[RFID]);

PRINT 'Post-deployment validation:';
PRINT '  Customer Lists: ' + CAST(@ListCount AS VARCHAR(10));
PRINT '  RFID Tags: ' + CAST(@RfidCount AS VARCHAR(10));

-- Log deployment completion
INSERT INTO sys.extended_properties 
(
    name, value, level0type, level0name, level1type, level1name, level2type, level2name
)
SELECT 
    'MS_Description', 
    'Last deployment completed on ' + CONVERT(VARCHAR, GETDATE(), 120) + ' by ' + SYSTEM_USER,
    'schema', 'dbo', NULL, NULL, NULL, NULL
WHERE NOT EXISTS (
    SELECT 1 FROM sys.extended_properties 
    WHERE name = 'MS_Description' AND major_id = 0 AND minor_id = 0
);

PRINT 'Post-deployment configuration completed successfully.';
PRINT 'Database is ready for use.';

-- Display connection information
PRINT 'Database: ' + DB_NAME();
PRINT 'Server: ' + @@SERVERNAME;
PRINT 'Deployment completed at: ' + CONVERT(VARCHAR, GETDATE(), 120);
