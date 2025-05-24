CREATE VIEW [dbo].[vw_RfidTagsWithListInfo]
AS
SELECT 
    r.[Id] AS RfidTagId,
    r.[RFID] AS RfidIdentifier,
    r.[Name] AS RfidTagName,
    r.[Description] AS RfidTagDescription,
    r.[Color],
    r.[Size],
    r.[CreatedAt] AS RfidTagCreatedAt,
    r.[UpdatedAt] AS RfidTagUpdatedAt,
    l.[Id] AS ListId,
    l.[Name] AS ListName,
    l.[Description] AS ListDescription,
    l.[SystemRef] AS ListSystemRef,
    l.[CreatedAt] AS ListCreatedAt,
    l.[UpdatedAt] AS ListUpdatedAt
FROM [dbo].[RFID] r
INNER JOIN [dbo].[List] l ON r.[ListId] = l.[Id];

GO

-- Add view documentation
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'View combining RFID tag information with their associated customer list details for reporting and querying', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'VIEW', @level1name = N'vw_RfidTagsWithListInfo';
