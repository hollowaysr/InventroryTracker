CREATE PROCEDURE [dbo].[sp_SearchRfidTags]
    @SearchTerm NVARCHAR(100) = NULL,
    @ListId INT = NULL,
    @Color NVARCHAR(50) = NULL,
    @Size NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate input parameters
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 OR @PageSize > 1000 SET @PageSize = 50;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Build dynamic search query
    SELECT 
        r.[Id],
        r.[RFID],
        r.[Name],
        r.[Description],
        r.[Color],
        r.[Size],
        r.[CreatedAt],
        r.[UpdatedAt],
        l.[Id] AS ListId,
        l.[Name] AS ListName,
        l.[Description] AS ListDescription,
        l.[SystemRef] AS ListSystemRef
    FROM [dbo].[RFID] r
    INNER JOIN [dbo].[List] l ON r.[ListId] = l.[Id]
    WHERE 
        (@SearchTerm IS NULL OR 
         r.[Name] LIKE '%' + @SearchTerm + '%' OR 
         r.[Description] LIKE '%' + @SearchTerm + '%' OR 
         r.[RFID] LIKE '%' + @SearchTerm + '%' OR
         l.[Name] LIKE '%' + @SearchTerm + '%')
    AND (@ListId IS NULL OR r.[ListId] = @ListId)
    AND (@Color IS NULL OR r.[Color] = @Color)
    AND (@Size IS NULL OR r.[Size] = @Size)
    ORDER BY r.[Name], r.[CreatedAt]
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Return total count for pagination
    SELECT COUNT(*) AS TotalCount
    FROM [dbo].[RFID] r
    INNER JOIN [dbo].[List] l ON r.[ListId] = l.[Id]
    WHERE 
        (@SearchTerm IS NULL OR 
         r.[Name] LIKE '%' + @SearchTerm + '%' OR 
         r.[Description] LIKE '%' + @SearchTerm + '%' OR 
         r.[RFID] LIKE '%' + @SearchTerm + '%' OR
         l.[Name] LIKE '%' + @SearchTerm + '%')
    AND (@ListId IS NULL OR r.[ListId] = @ListId)
    AND (@Color IS NULL OR r.[Color] = @Color)
    AND (@Size IS NULL OR r.[Size] = @Size);
END;

GO

-- Add procedure documentation
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Advanced search procedure for RFID tags with filtering, pagination, and full-text search capabilities', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'PROCEDURE', @level1name = N'sp_SearchRfidTags';
