USE MusicStreamingDB;
GO

PRINT '========================================';
PRINT 'FIX ALL TABLES: UpdatedAt & UpdatedBy';
PRINT '========================================';

-- 1. Artists
IF EXISTS (SELECT * FROM sys.columns 
           WHERE object_id = OBJECT_ID('Artists') 
           AND name = 'UpdatedAt' 
           AND is_nullable = 0)
BEGIN
    ALTER TABLE Artists ALTER COLUMN UpdatedAt DATETIME2 NULL;
    ALTER TABLE Artists ALTER COLUMN UpdatedBy UNIQUEIDENTIFIER NULL;
    PRINT '✅ Artists fixed';
END

-- 2. Songs
IF EXISTS (SELECT * FROM sys.columns 
           WHERE object_id = OBJECT_ID('Songs') 
           AND name = 'UpdatedAt' 
           AND is_nullable = 0)
BEGIN
    ALTER TABLE Songs ALTER COLUMN UpdatedAt DATETIME2 NULL;
    ALTER TABLE Songs ALTER COLUMN UpdatedBy UNIQUEIDENTIFIER NULL;
    PRINT '✅ Songs fixed';
END

-- 3. Albums (nếu có)
IF EXISTS (SELECT * FROM sys.columns 
           WHERE object_id = OBJECT_ID('Albums') 
           AND name = 'UpdatedAt' 
           AND is_nullable = 0)
BEGIN
    ALTER TABLE Albums ALTER COLUMN UpdatedAt DATETIME2 NULL;
    ALTER TABLE Albums ALTER COLUMN UpdatedBy UNIQUEIDENTIFIER NULL;
    PRINT '✅ Albums fixed';
END

-- 4. Users (nếu có)
IF EXISTS (SELECT * FROM sys.columns 
           WHERE object_id = OBJECT_ID('Users') 
           AND name = 'UpdatedAt' 
           AND is_nullable = 0)
BEGIN
    ALTER TABLE Users ALTER COLUMN UpdatedAt DATETIME2 NULL;
    ALTER TABLE Users ALTER COLUMN UpdatedBy UNIQUEIDENTIFIER NULL;
    PRINT '✅ Users fixed';
END

-- 5. Playlists (nếu có)
IF EXISTS (SELECT * FROM sys.columns 
           WHERE object_id = OBJECT_ID('Playlists') 
           AND name = 'UpdatedAt' 
           AND is_nullable = 0)
BEGIN
    ALTER TABLE Playlists ALTER COLUMN UpdatedAt DATETIME2 NULL;
    ALTER TABLE Playlists ALTER COLUMN UpdatedBy UNIQUEIDENTIFIER NULL;
    PRINT '✅ Playlists fixed';
END

PRINT '';
PRINT '========================================';
PRINT '✅ ALL TABLES FIXED!';
PRINT '========================================';

-- Hiển thị tóm tắt
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    IS_NULLABLE,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME IN ('UpdatedAt', 'UpdatedBy')
ORDER BY TABLE_NAME, COLUMN_NAME;