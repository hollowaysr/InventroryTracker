CREATE PROCEDURE [dbo].[sp_GetRfidTagsByListId]
    @ListId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate input parameters
    IF @ListId IS NULL OR @ListId <= 0
    BEGIN
        RAISERROR('ListId must be a positive integer', 16, 1);
        RETURN;
    END

    -- Check if the list exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[List] WHERE [Id] = @ListId)
    BEGIN
        RAISERROR('List with ID %d does not exist', 16, 1, @ListId);
        RETURN;
    END

    -- Return RFID tags for the specified list
    SELECT 
        r.[Id],
        r.[RFID],
        r.[Name],
        r.[Description],
        r.[Color],
        r.[Size],
        r.[CreatedAt],
        r.[UpdatedAt],
        l.[Name] AS ListName,
        l.[Description] AS ListDescription
    FROM [dbo].[RFID] r
    INNER JOIN [dbo].[List] l ON r.[ListId] = l.[Id]
    WHERE r.[ListId] = @ListId
    ORDER BY r.[Name], r.[CreatedAt];

    -- Return count for information
    SELECT COUNT(*) AS TotalTags FROM [dbo].[RFID] WHERE [ListId] = @ListId;
END;

GO

-- Add procedure documentation
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Retrieves all RFID tags associated with a specific customer list, including list information and tag count', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'PROCEDURE', @level1name = N'sp_GetRfidTagsByListId';
