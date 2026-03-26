-- =============================================
-- HỆ THỐNG STREAMING NHẠC - KHỞI TẠO DATABASE
-- Mô tả: Phiên bản MVP cho dự án 3 tuần
-- Tổng cộng: 10 Bảng, 5 Triggers, 8 Stored Procedures
-- =============================================

--USE master;
--GO

-- Xóa DB cũ nếu đã tồn tại để chạy lại từ đầu
--IF EXISTS (SELECT * FROM sys.databases WHERE name = 'MusicStreamingDB')
--BEGIN
--    ALTER DATABASE MusicStreamingDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
--    DROP DATABASE MusicStreamingDB;
--END
--GO

--CREATE DATABASE MusicStreamingDB;
--GO

USE MusicStreamingDB;
GO

-- 1. BẢNG NGƯỜI DÙNG (Users)
-- Lưu trữ: Khách hàng, Manager, Admin
CREATE TABLE Users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Username NVARCHAR(50) NOT NULL UNIQUE,      -- Tên đăng nhập
    Email NVARCHAR(255) NOT NULL UNIQUE,       -- Email dùng để reset pass/liên lạc
    PasswordHash NVARCHAR(255) NOT NULL,       -- Mật khẩu đã mã hóa
    FullName NVARCHAR(100),                    -- Tên đầy đủ
    Avatar NVARCHAR(500),                      -- Link ảnh đại diện
    
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active', -- Trạng thái: Active (Hoạt động)/Disabled (Bị khóa)
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(), -- Ngày tạo tài khoản
    LastLoginAt DATETIME2,                         -- Lần cuối đăng nhập
    
    INDEX IX_Users_Email (Email),
    INDEX IX_Users_Username (Username)
);
GO

-- 2. BẢNG VAI TRÒ (Roles)
CREATE TABLE Roles (
    RoleId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RoleName NVARCHAR(50) NOT NULL UNIQUE, -- Ví dụ: 'Admin', 'Manager', 'Customer'
    Description NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 3. BẢNG PHÂN QUYỀN (UserRoles) - Quan hệ nhiều-nhiều giữa User và Role
CREATE TABLE UserRoles (
    UserRoleId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    AssignedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) ON DELETE CASCADE,
    CONSTRAINT UQ_UserRoles UNIQUE (UserId, RoleId) -- Một user không được trùng vai trò
);
GO

-- 4. BẢNG NGHỆ SĨ (Artists)
CREATE TABLE Artists (
    ArtistId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ArtistName NVARCHAR(200) NOT NULL,
    AvatarUrl NVARCHAR(500),
    Biography NVARCHAR(1000), -- Tiểu sử nghệ sĩ
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy UNIQUEIDENTIFIER, -- Người cuối cùng chỉnh sửa (Manager)
    FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId)
);
GO

-- 5. BẢNG BÀI HÁT (Songs) - Bảng quan trọng nhất
CREATE TABLE Songs (
    SongId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(255) NOT NULL,             -- Tiêu đề bài hát
    Duration INT NOT NULL,                    -- Thời lượng (giây)
    Lyrics NVARCHAR(MAX),                     -- Lời bài hát
    
    FileUrl NVARCHAR(1000) NOT NULL,          -- Link file nhạc trên Cloud (Google Storage)
    FileSize BIGINT,                          -- Kích thước file (bytes)
    Format NVARCHAR(20) DEFAULT 'MP3',        -- Định dạng: MP3/FLAC...
    
    Genre NVARCHAR(100),                      -- Thể loại: Pop, Rock, Rap...
    ReleaseYear INT,                          -- Năm phát hành
    CoverImageUrl NVARCHAR(500),              -- Link ảnh bìa album/bài hát
    
    TotalPlays BIGINT NOT NULL DEFAULT 0,     -- Tổng lượt nghe (Trigger tự cập nhật)
    TotalLikes INT NOT NULL DEFAULT 0,        -- Tổng lượt thích (Trigger tự cập nhật)
    
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Trạng thái: Pending (Chờ duyệt)/Active (Đã duyệt)
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy UNIQUEIDENTIFIER,               -- Người upload bài hát
    ApprovedBy UNIQUEIDENTIFIER,              -- Manager thực hiện duyệt bài
    ApprovedAt DATETIME2,                     -- Ngày duyệt
    
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    FOREIGN KEY (ApprovedBy) REFERENCES Users(UserId)
);
GO

-- 6. BẢNG NGHỆ SĨ - BÀI HÁT (SongArtists)
-- Vì 1 bài có thể có nhiều ca sĩ (Feat)
CREATE TABLE SongArtists (
    SongArtistId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SongId UNIQUEIDENTIFIER NOT NULL,
    ArtistId UNIQUEIDENTIFIER NOT NULL,
    IsPrimary BIT NOT NULL DEFAULT 1, -- Ca sĩ chính hay ca sĩ phụ (featured)
    
    FOREIGN KEY (SongId) REFERENCES Songs(SongId) ON DELETE CASCADE,
    FOREIGN KEY (ArtistId) REFERENCES Artists(ArtistId) ON DELETE CASCADE
);
GO

-- 7. BẢNG PLAYLIST (Playlists)
CREATE TABLE Playlists (
    PlaylistId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    IsPublic BIT NOT NULL DEFAULT 1, -- Chế độ công khai hay cá nhân
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);
GO

-- 8. BẢNG NHẠC TRONG PLAYLIST (PlaylistSongs)
CREATE TABLE PlaylistSongs (
    PlaylistSongId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PlaylistId UNIQUEIDENTIFIER NOT NULL,
    SongId UNIQUEIDENTIFIER NOT NULL,
    AddedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    FOREIGN KEY (PlaylistId) REFERENCES Playlists(PlaylistId) ON DELETE CASCADE,
    FOREIGN KEY (SongId) REFERENCES Songs(SongId) ON DELETE CASCADE
);
GO

-- 9. BẢNG YÊU THÍCH (SongLikes)
CREATE TABLE SongLikes (
    SongLikeId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    SongId UNIQUEIDENTIFIER NOT NULL,
    LikedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (SongId) REFERENCES Songs(SongId) ON DELETE CASCADE
);
GO

-- 10. BẢNG LỊCH SỬ NGHE (ListeningHistory)
CREATE TABLE ListeningHistory (
    HistoryId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    SongId UNIQUEIDENTIFIER NOT NULL,
    PlayedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (SongId) REFERENCES Songs(SongId) ON DELETE CASCADE
);
GO

-- =============================================
-- TRIGGERS: TỰ ĐỘNG CẬP NHẬT THỐNG KÊ
-- =============================================

-- Tự động tăng TotalPlays khi có người nghe mới
CREATE TRIGGER trg_UpdateSongPlays ON ListeningHistory AFTER INSERT AS
BEGIN
    UPDATE Songs SET TotalPlays = TotalPlays + 1 
    WHERE SongId IN (SELECT DISTINCT SongId FROM inserted);
END;
GO

-- Tự động tăng TotalLikes khi có người nhấn Like
CREATE TRIGGER trg_UpdateSongLikes_Insert ON SongLikes AFTER INSERT AS
BEGIN
    UPDATE Songs SET TotalLikes = TotalLikes + 1 
    WHERE SongId IN (SELECT DISTINCT SongId FROM inserted);
END;
GO

-- Tự động giảm TotalLikes khi người dùng bỏ Like (Delete bản ghi)
CREATE TRIGGER trg_UpdateSongLikes_Delete ON SongLikes AFTER DELETE AS
BEGIN
    UPDATE Songs SET TotalLikes = TotalLikes - 1 
    WHERE SongId IN (SELECT DISTINCT SongId FROM deleted);
END;
GO

-- =============================================
-- STORED PROCEDURES: CÁC HÀM XỬ LÝ NHANH
-- =============================================

-- Lấy danh sách bài hát nổi bật (Trending) dựa trên lượt nghe gần đây
CREATE PROCEDURE sp_GetTrendingSongs
    @TopN INT = 20,
    @DaysBack INT = 7
AS
BEGIN
    SELECT TOP (@TopN) s.SongId, s.Title, s.CoverImageUrl, s.TotalPlays,
           STRING_AGG(a.ArtistName, ', ') AS Artists
    FROM Songs s
    JOIN SongArtists sa ON s.SongId = sa.SongId
    JOIN Artists a ON sa.ArtistId = a.ArtistId
    WHERE s.Status = 'Active'
    GROUP BY s.SongId, s.Title, s.CoverImageUrl, s.TotalPlays
    ORDER BY s.TotalPlays DESC;
END;
GO

PRINT 'Khởi tạo Database MusicStreamingDB thành công!';






-- Thêm cột UpdatedAt vào bảng Artists
ALTER TABLE Artists
ADD UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
GO

-- Update giá trị hiện tại (set = CreatedAt)
UPDATE Artists
SET UpdatedAt = CreatedAt
WHERE UpdatedAt IS NULL;
GO

PRINT 'Added UpdatedAt column to Artists table';



-- Thêm cột lý do vào bảng Song
ALTER TABLE Songs ADD RejectReason NVARCHAR(500) NULL;
