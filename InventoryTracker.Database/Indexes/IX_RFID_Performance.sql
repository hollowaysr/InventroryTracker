-- Performance indexes for RFID table
CREATE NONCLUSTERED INDEX [IX_RFID_CreatedAt] 
ON [dbo].[RFID] ([CreatedAt] DESC)
INCLUDE ([RFID], [Name], [ListId]);

CREATE NONCLUSTERED INDEX [IX_RFID_UpdatedAt] 
ON [dbo].[RFID] ([UpdatedAt] DESC)
WHERE [UpdatedAt] IS NOT NULL
INCLUDE ([Id], [RFID], [Name]);

CREATE NONCLUSTERED INDEX [IX_RFID_Color_Size] 
ON [dbo].[RFID] ([Color], [Size])
WHERE [Color] IS NOT NULL OR [Size] IS NOT NULL
INCLUDE ([Id], [RFID], [Name], [ListId]);

CREATE NONCLUSTERED INDEX [IX_RFID_Name_Description] 
ON [dbo].[RFID] ([Name], [Description])
INCLUDE ([Id], [RFID], [ListId], [Color], [Size]);

-- Composite index for list-based queries with metadata filtering
CREATE NONCLUSTERED INDEX [IX_RFID_ListId_Color_Size] 
ON [dbo].[RFID] ([ListId], [Color], [Size])
INCLUDE ([Id], [RFID], [Name], [Description], [CreatedAt]);

-- Covering index for search operations
CREATE NONCLUSTERED INDEX [IX_RFID_Search_Covering] 
ON [dbo].[RFID] ([ListId], [Name])
INCLUDE ([Id], [RFID], [Description], [Color], [Size], [CreatedAt], [UpdatedAt]);

-- Full-text search optimization (if full-text search is enabled)
-- CREATE FULLTEXT INDEX ON [dbo].[RFID] ([Name], [Description]) 
-- KEY INDEX [PK_RFID] WITH STOPLIST = SYSTEM;

GO

-- Add index documentation
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Performance indexes for RFID table to optimize search, filtering, and join operations with List table', 
    @level0type = N'SCHEMA', @level0name = N'dbo';
