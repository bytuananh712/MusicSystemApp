using Microsoft.Extensions.Logging;
using MusicSystem.Application.Services.Artists;
using MusicSystem.Application.Services.Auth;
using MusicSystem.Application.Services.Files;
using MusicSystem.Application.Services.Songs;
using MusicSystem.Application.Services.Users;
using MusicSystem.Domain.Interfaces;
using MusicSystem.Infrastructure.Data;
using MusicSystem.Shared.Constants;
using MusicSystem.Shared.DTOs.Artists;
using MusicSystem.Shared.DTOs.Auth;
using MusicSystem.Shared.DTOs.Songs;
using MusicSystem.Shared.DTOs.Users;
using MusicSystem.Shared.SocketContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MusicSystem.Infrastructure.Socket
{
    public class SocketHandler
    {
        private readonly ILogger<SocketHandler> _logger;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IArtistService _artistService;
        private readonly ISongService _songService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IUserRepository _userRepository;
        private readonly MusicStreamingDbContext _dbContext;

        // Lưu giữ UserId của phiên đăng nhập hiện tại (per-connection)
        private Guid? _authenticatedUserId;
        private List<string> _authenticatedRoles = new List<string>();

        public SocketHandler(
            ILogger<SocketHandler> logger,
            IAuthService authService,
            IUserService userService,
            IArtistService artistService,
            ISongService songService,
            IFileUploadService fileUploadService,
            IUserRepository userRepository,
            MusicStreamingDbContext dbContext)
        {
            _logger = logger;
            _authService = authService;
            _userService = userService;
            _artistService = artistService;
            _songService = songService;
            _fileUploadService = fileUploadService;
            _userRepository = userRepository;
            _dbContext = dbContext;
        }

        public async Task HandleAsync(TcpClient client, CancellationToken cancellationToken)
        {
            var clientEndpoint = client.Client.RemoteEndPoint?.ToString();
            _logger.LogInformation(" Client connected: {ClientEndpoint}", clientEndpoint);

            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                try
                {
                    var requestJson = await reader.ReadLineAsync();

                    if (string.IsNullOrEmpty(requestJson))
                    {
                        _logger.LogWarning(" Empty request received");
                        break;
                    }

                    _logger.LogDebug(" Received: {RequestJson}", requestJson);

                    var request = JsonSerializer.Deserialize<SocketRequest>(requestJson);

                    _logger.LogInformation(" Command: {Command}", request.Command);

                    var response = await ProcessRequestAsync(request);

                    var responseJson = JsonSerializer.Serialize(response);
                    await writer.WriteLineAsync(responseJson);

                    _logger.LogInformation(" Status: {Status}", response.Status);
                    _logger.LogDebug(" Sent: {ResponseJson}", responseJson);
                }
                catch (IOException ioEx)
                {
                    _logger.LogWarning(ioEx, " Client disconnected");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, " Error processing request");

                    var errorResponse = new SocketResponse
                    {
                        Status = SocketStatus.Error,
                        Message = $"Server error: {ex.Message}",
                        Timestamp = DateTime.UtcNow
                    };

                    var errorJson = JsonSerializer.Serialize(errorResponse);
                    await writer.WriteLineAsync(errorJson);
                }
            }

            _logger.LogInformation(" Client disconnected: {ClientEndpoint}", clientEndpoint);
        }

        // ==================== PROCESS REQUEST (CÓ XÁC THỰC) ====================

        private async Task<SocketResponse> ProcessRequestAsync(SocketRequest request)
        {
            try
            {
                //  Các command KHÔNG cần xác thực
                if (request.Command == SocketCommands.Login)
                {
                    return await HandleLoginAsync(request);
                }

                //  TẤT CẢ các command khác ĐỀU cần Token hợp lệ
                var userId = _authService.ValidateSimpleToken(request.Token);
                if (userId == null)
                {
                    _logger.LogWarning(" Unauthorized request: Command={Command}, Token is invalid or missing", request.Command);
                    return UnauthorizedResponse(request.RequestId, "Token không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.");
                }

                //  Lưu UserId vào phiên hiện tại
                _authenticatedUserId = userId.Value;

                //  Lấy roles của user từ DB để kiểm tra quyền
                await LoadUserRolesAsync(userId.Value);

                //  Kiểm tra quyền: Chỉ Admin và Manager mới được dùng Portal
                if (!_authenticatedRoles.Contains("Admin") && !_authenticatedRoles.Contains("Manager"))
                {
                    _logger.LogWarning(" Forbidden: User {UserId} with roles [{Roles}] tried to access admin command",
                        userId.Value, string.Join(", ", _authenticatedRoles));
                    return UnauthorizedResponse(request.RequestId, "Bạn không có quyền truy cập chức năng này.");
                }

                return request.Command switch
                {
                    SocketCommands.ValidateToken => HandleValidateToken(request),
                    SocketCommands.Logout => HandleLogout(request),

                    // Admin - Quản lý User (chỉ Admin)
                    SocketCommands.GetAllUsers => await RequireRole("Admin", request, () => HandleGetAllUsersAsync(request)),
                    SocketCommands.CreateUser => await RequireRole("Admin", request, () => HandleCreateUserAsync(request)),
                    SocketCommands.UpdateUser => await RequireRole("Admin", request, () => HandleUpdateUserAsync(request)),
                    SocketCommands.DisableUser => await RequireRole("Admin", request, () => HandleDisableUserAsync(request)),
                    SocketCommands.EnableUser => await RequireRole("Admin", request, () => HandleEnableUserAsync(request)),
                    SocketCommands.AssignRole => await RequireRole("Admin", request, () => HandleAssignRoleAsync(request)),
                    SocketCommands.ResetPassword => await RequireRole("Admin", request, () => HandleResetPasswordAsync(request)),

                    // Artist - Admin & Manager đều được
                    SocketCommands.GetAllArtists => await HandleGetAllArtistsAsync(request),
                    SocketCommands.CreateArtist => await HandleCreateArtistAsync(request),
                    SocketCommands.UpdateArtist => await HandleUpdateArtistAsync(request),
                    SocketCommands.DeleteArtist => await HandleDeleteArtistAsync(request),
                    SocketCommands.DisableArtist => await HandleDisableArtistAsync(request),
                    SocketCommands.EnableArtist => await HandleEnableArtistAsync(request),

                    // Song - Admin & Manager đều được
                    SocketCommands.GetAllSongs => await HandleGetAllSongsAsync(request),
                    SocketCommands.GetPendingSongs => await HandleGetPendingSongsAsync(request),
                    SocketCommands.UploadSongFile => await HandleUploadSongFileAsync(request),
                    SocketCommands.CreateSong => await HandleCreateSongAsync(request),
                    SocketCommands.UpdateSong => await HandleUpdateSongAsync(request),
                    SocketCommands.DeleteSong => await HandleDeleteSongAsync(request),

                    // Admin ONLY (Sửa lỗi phân quyền duyệt bài do Manager gọi qua mặt Admin)
                    SocketCommands.ApproveSong => await RequireRole("Admin", request, () => HandleApproveSongAsync(request)),
                    SocketCommands.RejectSong => await RequireRole("Admin", request, () => HandleRejectSongAsync(request)),

                    _ => new SocketResponse
                    {
                        RequestId = request.RequestId,
                        Status = SocketStatus.InvalidRequest,
                        Message = $"Unknown command: {request.Command}",
                        Timestamp = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Command error: {Command}", request.Command);
                return ErrorResponse(request.RequestId, ex.Message);
            }
            finally
            {
                // XÓA TRACKING của EF Core sau mỗi request để lấy dữ liệu mới ở các request sau!
                // Fix lỗi 2 app đang bật thì một bên duyệt, một bên không cập nhật được.
                _dbContext?.ChangeTracker?.Clear();
            }
        }

        // ==================== ROLE-BASED ACCESS CONTROL ====================


        /// Kiểm tra người dùng hiện tại có role yêu cầu không.
        /// Nếu không, trả về Unauthorized.

        private async Task<SocketResponse> RequireRole(string requiredRole, SocketRequest request, Func<Task<SocketResponse>> handler)
        {
            if (!_authenticatedRoles.Contains(requiredRole))
            {
                _logger.LogWarning(" Forbidden: User {UserId} cần role '{Role}' nhưng chỉ có [{Roles}]",
                    _authenticatedUserId, requiredRole, string.Join(", ", _authenticatedRoles));
                return UnauthorizedResponse(request.RequestId, $"Chức năng này yêu cầu quyền {requiredRole}.");
            }

            return await handler();
        }


        /// Load danh sách Roles của user từ DB

        private async Task LoadUserRolesAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user?.UserRoles != null)
            {
                _authenticatedRoles = user.UserRoles
                    .Where(ur => ur.Role != null)
                    .Select(ur => ur.Role!.RoleName ?? string.Empty)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();
            }
            else
            {
                _authenticatedRoles = new List<string>();
            }
        }

        // ==================== DTO VALIDATION ====================


        /// Validate DTO bằng DataAnnotations.
        /// Trả về danh sách lỗi nếu có, hoặc null nếu hợp lệ.

        private List<string>? ValidateDto<T>(T? dto)
        {
            if (dto == null)
                return new List<string> { "Dữ liệu gửi lên không hợp lệ hoặc bị thiếu." };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

            if (!isValid)
            {
                return results.Select(r => r.ErrorMessage ?? "Lỗi validation").ToList();
            }

            return null; // Hợp lệ
        }


        /// Validate DTO và trả về ErrorResponse nếu không hợp lệ.
        /// Trả về null nếu hợp lệ (để tiếp tục xử lý).

        private SocketResponse? ValidateDtoOrError<T>(Guid requestId, T? dto)
        {
            var errors = ValidateDto(dto);
            if (errors != null && errors.Any())
            {
                var errorMessage = "Dữ liệu không hợp lệ:\n• " + string.Join("\n• ", errors);
                _logger.LogWarning(" Validation failed: {Errors}", errorMessage);
                return ErrorResponse(requestId, errorMessage);
            }
            return null; // OK
        }

        // ==================== AUTH HANDLERS ====================

        private async Task<SocketResponse> HandleLoginAsync(SocketRequest request)
        {
            try
            {
                var loginRequest = JsonSerializer.Deserialize<LoginRequestDto>(request.Data);

                //  Validate DTO
                var validationError = ValidateDtoOrError(request.RequestId, loginRequest);
                if (validationError != null) return validationError;

                _logger.LogInformation(" Login attempt: {Username}", loginRequest.Username);

                var loginResult = await _authService.LoginAsync(loginRequest);

                if (!loginResult.Success)
                {
                    _logger.LogWarning(" Login failed: {Message}", loginResult.Message);
                    return UnauthorizedResponse(request.RequestId, loginResult.Message);
                }

                if (!loginResult.User.Roles.Contains("Admin") && !loginResult.User.Roles.Contains("Manager"))
                {
                    _logger.LogWarning(" Unauthorized role: {Roles}", string.Join(", ", loginResult.User.Roles));
                    return UnauthorizedResponse(request.RequestId, "Chỉ Admin và Manager mới có thể đăng nhập vào ứng dụng quản trị");
                }

                //  Lưu phiên sau khi login thành công
                _authenticatedUserId = loginResult.User.UserId;
                _authenticatedRoles = loginResult.User.Roles;

                _logger.LogInformation(" Login successful: {Username} ({Roles})",
                    loginResult.User.Username, string.Join(", ", loginResult.User.Roles));

                return SuccessResponse(request.RequestId, loginResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Login error");
                return ErrorResponse(request.RequestId, $"Lỗi đăng nhập: {ex.Message}");
            }
        }

        // ==================== ARTIST HANDLERS ====================

        private async Task<SocketResponse> HandleGetAllArtistsAsync(SocketRequest request)
        {
            var artists = await _artistService.GetAllArtistsAsync();
            var artistList = artists.ToList();

            _logger.LogInformation(" Found {Count} artists", artistList.Count);

            return SuccessResponse(request.RequestId, artistList);
        }

        private async Task<SocketResponse> HandleCreateArtistAsync(SocketRequest request)
        {
            var createDto = JsonSerializer.Deserialize<CreateArtistDto>(request.Data);

            //  Validate DTO
            var validationError = ValidateDtoOrError(request.RequestId, createDto);
            if (validationError != null) return validationError;

            _logger.LogInformation(" Creating artist: {ArtistName}", createDto.ArtistName);

            var artist = await _artistService.CreateArtistAsync(createDto);

            _logger.LogInformation(" Artist created: {ArtistName}", artist.ArtistName);

            return SuccessResponse(request.RequestId, artist);
        }

        // ==================== SONG HANDLERS ====================

        private async Task<SocketResponse> HandleGetPendingSongsAsync(SocketRequest request)
        {
            var songs = await _songService.GetPendingSongsAsync();
            var songList = songs.ToList();

            _logger.LogInformation(" Found {Count} pending songs", songList.Count);

            return SuccessResponse(request.RequestId, songList);
        }

        private async Task<SocketResponse> HandleCreateSongAsync(SocketRequest request)
        {
            try
            {
                var createDto = JsonSerializer.Deserialize<CreateSongDto>(request.Data);

                //  Validate DTO
                var validationError = ValidateDtoOrError(request.RequestId, createDto);
                if (validationError != null) return validationError;

                //  Lấy UserId từ phiên đăng nhập thực tế
                var userId = GetCurrentUserId();

                _logger.LogInformation(" Creating song: {Title} by User {UserId}", createDto.Title, userId);

                var song = await _songService.CreateSongAsync(createDto, userId);

                _logger.LogInformation(" Song created: {Title}", song.Title);

                return SuccessResponse(request.RequestId, song);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error creating song");
                return ErrorResponse(request.RequestId, ex.Message);
            }
        }

        private async Task<SocketResponse> HandleApproveSongAsync(SocketRequest request)
        {
            var songId = Guid.Parse(request.Data);

            //  Lấy UserId từ phiên đăng nhập thực tế
            var adminId = GetCurrentUserId();

            _logger.LogInformation(" Approving song: {SongId} by Admin {AdminId}", songId, adminId);

            var result = await _songService.ApproveSongAsync(songId, adminId);

            _logger.LogInformation(" Song approved");

            return SuccessResponse(request.RequestId, result);
        }

        // ==================== HELPER METHODS ====================


        /// ✅ SỬA LỖI: Trả về UserId từ phiên đăng nhập thực tế thay vì query DB bừa bãi.

        private Guid GetCurrentUserId()
        {
            if (_authenticatedUserId == null)
                throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.");

            return _authenticatedUserId.Value;
        }

        private SocketResponse SuccessResponse(Guid requestId, object data)
        {
            return new SocketResponse
            {
                RequestId = requestId,
                Status = SocketStatus.Success,
                Message = "OK",
                Data = JsonSerializer.Serialize(data),
                Timestamp = DateTime.UtcNow
            };
        }

        private SocketResponse ErrorResponse(Guid requestId, string message)
        {
            return new SocketResponse
            {
                RequestId = requestId,
                Status = SocketStatus.Error,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        private SocketResponse UnauthorizedResponse(Guid requestId, string message)
        {
            return new SocketResponse
            {
                RequestId = requestId,
                Status = SocketStatus.Unauthorized,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        private SocketResponse HandleValidateToken(SocketRequest request)
        {
            //  Nếu code chạy đến đây nghĩa là Token đã được validate ở ProcessRequestAsync
            return SuccessResponse(request.RequestId, new { valid = true, userId = _authenticatedUserId });
        }

        private SocketResponse HandleLogout(SocketRequest request)
        {
            _logger.LogInformation(" Logout: User {UserId}", _authenticatedUserId);
            _authenticatedUserId = null;
            _authenticatedRoles = new List<string>();
            return SuccessResponse(request.RequestId, new { success = true });
        }

        // ==================== USER MANAGEMENT HANDLERS (ADMIN ONLY) ====================

        private async Task<SocketResponse> HandleGetAllUsersAsync(SocketRequest request)
        {
            var users = await _userService.GetAllUsersAsync();
            return SuccessResponse(request.RequestId, users);
        }

        private async Task<SocketResponse> HandleCreateUserAsync(SocketRequest request)
        {
            var createDto = JsonSerializer.Deserialize<CreateUserDto>(request.Data);

            //  Validate DTO
            var validationError = ValidateDtoOrError(request.RequestId, createDto);
            if (validationError != null) return validationError;

            var user = await _userService.CreateUserAsync(createDto);
            return SuccessResponse(request.RequestId, user);
        }

        private async Task<SocketResponse> HandleUpdateUserAsync(SocketRequest request)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(request.Data);
            var userId = Guid.Parse(data["userId"].ToString());
            var updateDto = JsonSerializer.Deserialize<UpdateUserDto>(data["data"].ToString());

            //  Validate DTO
            var validationError = ValidateDtoOrError(request.RequestId, updateDto);
            if (validationError != null) return validationError;

            var user = await _userService.UpdateUserAsync(userId, updateDto);
            return SuccessResponse(request.RequestId, user);
        }

        private async Task<SocketResponse> HandleDisableUserAsync(SocketRequest request)
        {
            var userId = Guid.Parse(request.Data);
            var result = await _userService.DisableUserAsync(userId);
            return SuccessResponse(request.RequestId, result);
        }

        private async Task<SocketResponse> HandleEnableUserAsync(SocketRequest request)
        {
            var userId = Guid.Parse(request.Data);
            var result = await _userService.EnableUserAsync(userId);
            return SuccessResponse(request.RequestId, result);
        }

        private async Task<SocketResponse> HandleAssignRoleAsync(SocketRequest request)
        {
            var dto = JsonSerializer.Deserialize<AssignRoleDto>(request.Data);

            //  Validate DTO
            var validationError = ValidateDtoOrError(request.RequestId, dto);
            if (validationError != null) return validationError;

            var result = await _userService.AssignRoleAsync(dto.UserId, dto.RoleName);
            return SuccessResponse(request.RequestId, result);
        }

        private async Task<SocketResponse> HandleResetPasswordAsync(SocketRequest request)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(request.Data);
            var userId = Guid.Parse(data["userId"]);
            var newPassword = data["newPassword"];

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                return ErrorResponse(request.RequestId, "Mật khẩu mới phải có ít nhất 6 ký tự.");
            }

            var result = await _userService.ResetPasswordAsync(userId, newPassword);
            return SuccessResponse(request.RequestId, result);
        }

        // ==================== ARTIST MANAGEMENT HANDLERS ====================

        private async Task<SocketResponse> HandleUpdateArtistAsync(SocketRequest request)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(request.Data);
            var artistId = Guid.Parse(data["artistId"].ToString());
            var updateDto = JsonSerializer.Deserialize<UpdateArtistDto>(data["data"].ToString());

            //  Validate DTO
            var validationError = ValidateDtoOrError(request.RequestId, updateDto);
            if (validationError != null) return validationError;

            var artist = await _artistService.UpdateArtistAsync(artistId, updateDto);
            return SuccessResponse(request.RequestId, artist);
        }

        private async Task<SocketResponse> HandleDeleteArtistAsync(SocketRequest request)
        {
            var artistId = Guid.Parse(request.Data);
            var result = await _artistService.DeleteArtistAsync(artistId);
            return SuccessResponse(request.RequestId, result);
        }

        private async Task<SocketResponse> HandleDisableArtistAsync(SocketRequest request)
        {
            var artistId = Guid.Parse(request.Data);
            var result = await _artistService.DisableArtistAsync(artistId);
            return SuccessResponse(request.RequestId, result);
        }

        private async Task<SocketResponse> HandleEnableArtistAsync(SocketRequest request)
        {
            var artistId = Guid.Parse(request.Data);
            var result = await _artistService.EnableArtistAsync(artistId);
            return SuccessResponse(request.RequestId, result);
        }

        // ==================== SONG MANAGEMENT HANDLERS ====================

        private async Task<SocketResponse> HandleGetAllSongsAsync(SocketRequest request)
        {
            var songs = await _songService.GetAllSongsAsync(null, 1, 1000);
            return SuccessResponse(request.RequestId, songs);
        }

        private async Task<SocketResponse> HandleUploadSongFileAsync(SocketRequest request)
        {
            var uploadRequest = JsonSerializer.Deserialize<UploadFileRequest>(request.Data);
            var result = await _fileUploadService.UploadSongFileAsync(uploadRequest);
            return SuccessResponse(request.RequestId, result);
        }

        private async Task<SocketResponse> HandleUpdateSongAsync(SocketRequest request)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(request.Data);
            var songId = Guid.Parse(data["songId"].ToString());
            var updateDto = JsonSerializer.Deserialize<UpdateSongDto>(data["data"].ToString());

            //  Validate DTO
            var validationError = ValidateDtoOrError(request.RequestId, updateDto);
            if (validationError != null) return validationError;

            var song = await _songService.UpdateSongAsync(songId, updateDto);
            return SuccessResponse(request.RequestId, song);
        }

        private async Task<SocketResponse> HandleDeleteSongAsync(SocketRequest request)
        {
            var songId = Guid.Parse(request.Data);
            var result = await _songService.DeleteSongAsync(songId);
            return SuccessResponse(request.RequestId, result);
        }

        private async Task<SocketResponse> HandleRejectSongAsync(SocketRequest request)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(request.Data);
            var songId = Guid.Parse(data["songId"]);
            var reason = data.ContainsKey("reason") ? data["reason"] : "Không đạt yêu cầu";

            //  Lấy UserId từ phiên đăng nhập thực tế
            var adminId = GetCurrentUserId();

            var result = await _songService.RejectSongAsync(songId, adminId, reason);
            return SuccessResponse(request.RequestId, result);
        }
    }
}