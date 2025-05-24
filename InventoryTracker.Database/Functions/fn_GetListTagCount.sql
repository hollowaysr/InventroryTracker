CREATE FUNCTION [dbo].[fn_GetListTagCount]
(
    @ListId INT
)
RETURNS INT
AS
BEGIN
    DECLARE @TagCount INT;

    SELECT @TagCount = COUNT(*)
    FROM [dbo].[RFID]
    WHERE [ListId] = @ListId;

    RETURN ISNULL(@TagCount, 0);
END;

GO

-- Add function documentation
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Returns the total count of RFID tags associated with a specific customer list', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'FUNCTION', @level1name = N'fn_GetListTagCount';
