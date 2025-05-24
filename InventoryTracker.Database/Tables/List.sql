CREATE TABLE [dbo].[List]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [SystemRef] NVARCHAR(50) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedAt] DATETIME2 NULL DEFAULT (GETUTCDATE()),
    [ledger_start_transaction_id] BIGINT NOT NULL,
    [ledger_end_transaction_id] BIGINT NULL,
    [ledger_start_sequence_number] BIGINT NOT NULL,
    [ledger_end_sequence_number] BIGINT NULL,
    CONSTRAINT [PK_List] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [IX_List_Name] UNIQUE NONCLUSTERED ([Name] ASC),
    INDEX [IX_List_SystemRef] NONCLUSTERED ([SystemRef] ASC)
);

-- Add table-level constraints
GO

-- Ensure Name is not empty or whitespace
ALTER TABLE [dbo].[List] 
ADD CONSTRAINT [CK_List_Name_NotEmpty] 
CHECK (LEN(LTRIM(RTRIM([Name]))) > 0);

-- Ensure SystemRef is unique when not null
CREATE UNIQUE NONCLUSTERED INDEX [IX_List_SystemRef_Unique] 
ON [dbo].[List] ([SystemRef]) 
WHERE [SystemRef] IS NOT NULL;

-- Add comments for documentation
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Customer lists containing RFID tags and metadata', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'List';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Unique identifier for the customer list', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'List',
    @level2type = N'COLUMN', @level2name = N'Id';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Name of the customer list (must be unique)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'List',
    @level2type = N'COLUMN', @level2name = N'Name';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Optional description of the customer list', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'List',
    @level2type = N'COLUMN', @level2name = N'Description';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'External system reference identifier', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'List',
    @level2type = N'COLUMN', @level2name = N'SystemRef';
