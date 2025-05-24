CREATE TABLE [dbo].[RFID]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [RFID] NVARCHAR(50) NOT NULL,
    [ListId] INT NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [Color] NVARCHAR(50) NULL,
    [Size] NVARCHAR(50) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedAt] DATETIME2 NULL DEFAULT (GETUTCDATE()),
    [ledger_start_transaction_id] BIGINT NOT NULL,
    [ledger_end_transaction_id] BIGINT NULL,
    [ledger_start_sequence_number] BIGINT NOT NULL,
    [ledger_end_sequence_number] BIGINT NULL,
    CONSTRAINT [PK_RFID] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RFID_List] FOREIGN KEY ([ListId]) REFERENCES [dbo].[List]([Id]) ON DELETE CASCADE,
    CONSTRAINT [IX_RFID_Rfid_Unique] UNIQUE NONCLUSTERED ([RFID] ASC),
    INDEX [IX_RFID_ListId] NONCLUSTERED ([ListId] ASC),
    INDEX [IX_RFID_Name] NONCLUSTERED ([Name] ASC)
);

-- Add table-level constraints
GO

-- Ensure RFID is not empty or whitespace
ALTER TABLE [dbo].[RFID] 
ADD CONSTRAINT [CK_RFID_Rfid_NotEmpty] 
CHECK (LEN(LTRIM(RTRIM([RFID]))) > 0);

-- Ensure Name is not empty or whitespace
ALTER TABLE [dbo].[RFID] 
ADD CONSTRAINT [CK_RFID_Name_NotEmpty] 
CHECK (LEN(LTRIM(RTRIM([Name]))) > 0);

-- Ensure Color values are from allowed list (if specified)
ALTER TABLE [dbo].[RFID] 
ADD CONSTRAINT [CK_RFID_Color_Valid] 
CHECK ([Color] IS NULL OR [Color] IN ('Red', 'Blue', 'Green', 'Yellow', 'Orange', 'Purple', 'Black', 'White', 'Gray', 'Pink'));

-- Ensure Size values are from allowed list (if specified)
ALTER TABLE [dbo].[RFID] 
ADD CONSTRAINT [CK_RFID_Size_Valid] 
CHECK ([Size] IS NULL OR [Size] IN ('Small', 'Medium', 'Large', 'XL', 'XXL'));

-- Add comments for documentation
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'RFID tags with metadata and relationships to customer lists', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'RFID';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Unique identifier for the RFID tag record', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'RFID',
    @level2type = N'COLUMN', @level2name = N'Id';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'RFID tag identifier (must be unique across all tags)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'RFID',
    @level2type = N'COLUMN', @level2name = N'RFID';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Reference to the customer list this tag belongs to', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'RFID',
    @level2type = N'COLUMN', @level2name = N'ListId';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Human-readable name for the RFID tag', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'RFID',
    @level2type = N'COLUMN', @level2name = N'Name';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Optional description of the RFID tag', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'RFID',
    @level2type = N'COLUMN', @level2name = N'Description';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Color attribute for categorization', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'RFID',
    @level2type = N'COLUMN', @level2name = N'Color';

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Size attribute for categorization', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'RFID',
    @level2type = N'COLUMN', @level2name = N'Size';
