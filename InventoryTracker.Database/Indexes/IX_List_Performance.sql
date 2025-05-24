-- Performance indexes for List table
CREATE NONCLUSTERED INDEX [IX_List_CreatedAt] 
ON [dbo].[List] ([CreatedAt] DESC)
INCLUDE ([Name], [Description], [SystemRef]);

CREATE NONCLUSTERED INDEX [IX_List_UpdatedAt] 
ON [dbo].[List] ([UpdatedAt] DESC)
WHERE [UpdatedAt] IS NOT NULL
INCLUDE ([Id], [Name]);

CREATE NONCLUSTERED INDEX [IX_List_Name_Description] 
ON [dbo].[List] ([Name], [Description])
INCLUDE ([Id], [SystemRef], [CreatedAt]);

-- Covering index for common search scenarios
CREATE NONCLUSTERED INDEX [IX_List_Covering] 
ON [dbo].[List] ([Name])
INCLUDE ([Id], [Description], [SystemRef], [CreatedAt], [UpdatedAt]);

GO

-- Add index documentation
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Performance indexes for List table to optimize common query patterns and search operations', 
    @level0type = N'SCHEMA', @level0name = N'dbo';
