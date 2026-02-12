-- =============================================
-- DỮ LIỆU MẪU (SEED DATA) - HỆ THỐNG NHẠC
-- Author: Tuan Anh
-- =============================================

USE MusicStreamingDB;
GO


-----------------------------------------------------------
-- 1. VAI TRÒ (Roles) - Cột: RoleId, RoleName, Description, CreatedAt
-----------------------------------------------------------
DECLARE @AdminRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @ManagerRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @CustomerRoleId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Roles (RoleId, RoleName, Description)
VALUES 
    (@AdminRoleId, 'Admin', N'Quyền cao nhất - quản trị toàn bộ hệ thống'),
    (@ManagerRoleId, 'Manager', N'Quản lý nội dung - duyệt nhạc, quản lý nghệ sĩ'),
    (@CustomerRoleId, 'Customer', N'Người nghe nhạc - tạo playlist, yêu thích bài hát');

-----------------------------------------------------------
-- 2. NGƯỜI DÙNG (Users) - Cột: UserId, Username, Email, PasswordHash, FullName, Avatar, Status, CreatedAt
-- PasswordHash tương ứng với mật khẩu: Admin@123
-----------------------------------------------------------
DECLARE @AdminUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @ManagerUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @Customer1Id UNIQUEIDENTIFIER = NEWID();


INSERT INTO Users (UserId, Username, Email, PasswordHash, FullName, Avatar, Status)
VALUES 
    (@AdminUserId, 'admin', 'admin@musicsystem.com','$2a$11$GYeCN029jX5mhMHYYWljP.MeLudW/bMgA6FekvANflO5KKXrubLni', N'Quản trị viên', '/storage/avatars/admin.png', 'Active'),
    (@ManagerUserId, 'manager', 'manager@musicsystem.com', '$2a$11$twTTro7G1uOAVs095TnEL.Id.VaezckUcIXHj5SOLfc/6ShcCkMna', N'Người duyệt nhạc', '/storage/avatars/manager.png', 'Active'),
    (@Customer1Id, 'user01', 'user01@gmail.com', '$2a$11$KH3EPJG9MKo2JySbPUrRg.QhH2VQxO3lXxRnHgwZBCaDearFZXBfe', N'Nguyễn Văn A', '/storage/avatars/user01.png', 'Active');

-----------------------------------------------------------
-- 3. GÁN QUYỀN (UserRoles) - Cột: UserRoleId, UserId, RoleId, AssignedAt
-----------------------------------------------------------
INSERT INTO UserRoles (UserId, RoleId) VALUES 
    (@AdminUserId, @AdminRoleId),
    (@ManagerUserId, @ManagerRoleId),
    (@Customer1Id, @CustomerRoleId);

-----------------------------------------------------------
-- 4. NGHỆ SĨ (Artists) - Cột: ArtistId, ArtistName, AvatarUrl, Biography, Status, CreatedAt, UpdatedBy
-----------------------------------------------------------
DECLARE @Artist1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Artist2Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO Artists (ArtistId, ArtistName, AvatarUrl, Biography, Status, UpdatedBy)
VALUES 
    (@Artist1Id, N'Sơn Tùng M-TP', '/storage/artists/sontung.jpg', N'Nghệ sĩ V-Pop hàng đầu Việt Nam', 'Active', @ManagerUserId),
    (@Artist2Id, N'Đen Vâu', '/storage/artists/denvau.jpg', N'Rapper mang phong cách đời thường, tự sự', 'Active', @ManagerUserId);

-----------------------------------------------------------
-- 5. BÀI HÁT (Songs) - Cột: SongId, Title, Duration, FileUrl, Genre, ReleaseYear, CoverImageUrl, Status, CreatedBy, ApprovedBy, ApprovedAt
-----------------------------------------------------------
DECLARE @Song1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Song2Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO Songs (SongId, Title, Duration, FileUrl, Genre, ReleaseYear, CoverImageUrl, Status, CreatedBy, ApprovedBy, ApprovedAt)
VALUES 
    (@Song1Id, N'Chúng Ta Của Hiện Tại', 302, '/storage/music/chung-ta-cua-hien-tai.mp3', 'V-Pop', 2020, '/storage/covers/chung-ta-cua-hien-tai.jpg', 'Active', @ManagerUserId, @AdminUserId, GETDATE()),
    (@Song2Id, N'Hai Triệu Năm', 215, '/storage/music/hai-trieu-nam.mp3', 'Rap Việt', 2019, '/storage/covers/hai-trieu-nam.jpg', 'Active', @ManagerUserId, @AdminUserId, GETDATE());

-----------------------------------------------------------
-- 6. LIÊN KẾT CA SĨ - BÀI HÁT (SongArtists) - Cột: SongArtistId, SongId, ArtistId, IsPrimary
-----------------------------------------------------------
INSERT INTO SongArtists (SongId, ArtistId, IsPrimary) VALUES 
    (@Song1Id, @Artist1Id, 1),
    (@Song2Id, @Artist2Id, 1);

-----------------------------------------------------------
-- 7. PLAYLIST & PLAYLIST SONGS
-----------------------------------------------------------
DECLARE @Playlist1Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO Playlists (PlaylistId, UserId, Title, IsPublic) 
VALUES (@Playlist1Id, @Customer1Id, N'Nhạc Chill Cuối Tuần', 1);

INSERT INTO PlaylistSongs (PlaylistId, SongId) 
VALUES (@Playlist1Id, @Song1Id), (@Playlist1Id, @Song2Id);

-----------------------------------------------------------
-- 8. YÊU THÍCH & LỊCH SỬ NGHE
-- Chú ý: Cấu trúc bảng của bạn dùng LikedAt (SongLikes) và PlayedAt (ListeningHistory)
-----------------------------------------------------------
INSERT INTO SongLikes (UserId, SongId, LikedAt) 
VALUES (@Customer1Id, @Song1Id, GETDATE());

INSERT INTO ListeningHistory (UserId, SongId, PlayedAt) 
VALUES (@Customer1Id, @Song1Id, GETDATE()), (@Customer1Id, @Song2Id, DATEADD(MINUTE, -10, GETDATE()));

PRINT N'Nạp dữ liệu mẫu thành công! Hãy test Login: admin / Admin@123';
GO

-- Xem kết quả nhanh
SELECT Title, TotalPlays, TotalLikes FROM Songs;






-- Update password 
--UPDATE Users 
--SET PasswordHash = '$2a$11$GYeCN029jX5mhMHYYWljP.MeLudW/bMgA6FekvANflO5KKXrubLni' 
--WHERE Username = 'admin';





--UPDATE Users 
--SET PasswordHash = '$2a$11$twTTro7G1uOAVs095TnEL.Id.VaezckUcIXHj5SOLfc/6ShcCkMna' 
--WHERE Username = 'manager';

--UPDATE Users 
--SET PasswordHash = '$2a$11$KH3EPJG9MKo2JySbPUrRg.QhH2VQxO3lXxRnHgwZBCaDearFZXBfe' 
--WHERE Username = 'user01';


